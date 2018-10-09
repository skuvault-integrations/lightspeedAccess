using System;
using System.Net;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Common;
using Netco.ActionPolicyServices;
using System.IO;
using System.Runtime.ExceptionServices;
using lightspeedAccess;

namespace LightspeedAccess.Misc
{
	public sealed class ThrottlerAsync
	{
		private readonly ThrottlingInfoItem _maxQuota;
		private readonly long _accountId;
		private readonly Func< Task > _delay;
		private readonly Func<Task> _delayOnThrottlingException;
		private readonly int _maxRetryCount;
		private readonly int _requestCost;

		private const int QuotaThreshold = 30;

		public ThrottlerAsync( ThrottlerConfig config )
		{
			this._maxQuota = config._maxQuota;
			this._delay = config._delay;
			this._maxRetryCount = config._maxRetryCount;
			this._accountId = config._accountId;
			this._requestCost = config._requestCost;
			this._delayOnThrottlingException = config._delayOnThrottlingException;

			this._throttlerActionPolicy = ActionPolicyAsync.Handle< Exception >().RetryAsync( this._maxRetryCount, async ( ex, i ) =>
			{
				if( !this.IsExceptionFromThrottling( ex ) )
				{
					var errMessage = string.Format( "Throttler: faced non-throttling exception: {0}", ex.Message );
					LightspeedLogger.Debug( errMessage, (int)this._accountId );

					if( LightspeedAuthService.IsUnauthorizedException( ex ) )
					{
						throw ex;
					}

					var webException = ex as WebException;
					if( webException != null )
					{
						var response = webException.Response as HttpWebResponse;
						if( response == null )
							throw new LightspeedException( errMessage, ex );

						string responseText = null;
						try
						{
							using( var reader = new StreamReader( response.GetResponseStream() ) )
							{
								responseText = reader.ReadToEnd();
							}
						}
						catch
						{
						}
						if( !string.IsNullOrWhiteSpace( responseText ) )
							throw new LightspeedException( responseText, ex );
					}

					throw new LightspeedException( errMessage, ex);
				}
				LightspeedLogger.Debug( "Throttler: got throttling exception. Retrying...", (int)this._accountId );
				await this._delayOnThrottlingException();
			})
			;
		}

		// default throttler that implements Lightspeed leaky bucket
		public ThrottlerAsync( long accountId ): this( ThrottlerConfig.CreateDefault( accountId ) )
		{
		}

		private readonly ActionPolicyAsync _throttlerActionPolicy; 
 
		public async Task< TResult > ExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			try
			{
				return await this._throttlerActionPolicy.Get( () => this.TryExecuteAsync( funcToThrottle ) );
			}
			catch( AggregateException ex )
			{
				ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
				throw;
			}
		}

		private async Task< TResult > TryExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			var semaphore = LightspeedGlobalThrottlingInfo.GetSemaphoreSync( this._accountId );
			await semaphore.WaitAsync();

			await this.WaitIfNeededAsync();
			
			TResult result;
			try
			{
				result = await funcToThrottle();
				LightspeedLogger.Debug( "Throttler: request executed successfully", (int)this._accountId );
				this.SubtractQuota( result );
			}
			finally
			{
				semaphore.Release();
			}
			
			return result;
		}

		private bool IsExceptionFromThrottling( Exception exception )
		{
			if( !( exception is WebException ) )
				return false;
			var webException = ( WebException ) exception;
			if( webException.Status != WebExceptionStatus.ProtocolError )
				return false;

			return webException.Response is HttpWebResponse && ( ( HttpWebResponse )webException.Response ).StatusCode == ( HttpStatusCode )429;
		}

		private ThrottlingInfoItem GetRemainingQuota()
		{
			ThrottlingInfoItem info;
			if( !LightspeedGlobalThrottlingInfo.GetThrottlingInfo( this._accountId, out info ) )
				info = this._maxQuota;
			return info;
		}

		private void SetRemainingQuota( int quota, float dripRate )
		{
			LightspeedGlobalThrottlingInfo.AddThrottlingInfo( this._accountId, new ThrottlingInfoItem( quota, dripRate ) );
		}

		private async Task WaitIfNeededAsync()
		{
			var remainingQuota = this.GetRemainingQuota();
			LightspeedLogger.Debug( string.Format( "Current quota for account {0} is: {1}", this._accountId, remainingQuota ), (int)this._accountId );

			if( remainingQuota.RemainingQuantity > this._requestCost )
			{
				// we set new remaining quota for case potential error (this is strange, but it worked so long time)
				remainingQuota = new ThrottlingInfoItem( remainingQuota.RemainingQuantity - this._requestCost, remainingQuota.DripRate );
				this.SetRemainingQuota( remainingQuota.RemainingQuantity > 0 ? remainingQuota.RemainingQuantity : 0, remainingQuota.DripRate );
				return;
			}

			var timeForDelay = Convert.ToInt32( Math.Ceiling( ( this._requestCost - remainingQuota.RemainingQuantity ) / remainingQuota.DripRate ) );

			LightspeedLogger.Debug( string.Format( "Throttler: quota exceeded. Waiting {0} seconds...", timeForDelay ), ( int )this._accountId );
			await Task.Delay( timeForDelay );
			LightspeedLogger.Debug( "Throttler: Resuming...", (int)this._accountId );			
		}

		private void SubtractQuota< TResult >( TResult result )
		{
			ResponseLeakyBucketMetadata bucketMetadata;
			LightspeedLogger.Debug( "Throttler: trying to get leaky bucket metadata from response", ( int )this._accountId );
			if( QuotaParser.TryParseQuota( result, out bucketMetadata ) )
			{
				LightspeedLogger.Debug( string.Format( "Throttler: parsed leaky bucket metadata from response. Bucket size: {0}. Used: {1}. Drip rate: {2}", bucketMetadata.quotaSize, bucketMetadata.quotaUsed, bucketMetadata.dripRate ), ( int )this._accountId );
				var quotaDelta = bucketMetadata.quotaSize - bucketMetadata.quotaUsed;
				this.SetRemainingQuota( quotaDelta > 0 ? quotaDelta : 0, bucketMetadata.dripRate );
			}

			var remainingQuota = this.GetRemainingQuota();
			LightspeedLogger.Debug( string.Format( "Throttler: substracted quota, now available {0}, drip rate {1}", remainingQuota.RemainingQuantity, remainingQuota.DripRate ), ( int )this._accountId );
		}

		public class ThrottlerException: Exception
		{
			public ThrottlerException()
			{
			}

			public ThrottlerException( string message )
				: base( message )
			{
			}

			public ThrottlerException( string message, Exception innerException )
				: base( message, innerException )
			{
			}
		}

		public class NonCriticalException: Exception
		{
			public NonCriticalException()
			{
			}

			public NonCriticalException( string message )
				: base( message )
			{
			}

			public NonCriticalException( string message, Exception innerException )
				: base( message, innerException )
			{
			}
		}
	}
}