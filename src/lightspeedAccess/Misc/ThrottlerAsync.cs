using System;
using System.Net;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using Netco.ActionPolicyServices;
using System.IO;
using System.Runtime.ExceptionServices;
using SkuVault.Integrations.Core.Common;

namespace LightspeedAccess.Misc
{
	public sealed class ThrottlerAsync
	{
		private readonly ThrottlingInfoItem _maxQuota;
		private readonly long _accountId;
		private readonly SyncRunContext _syncRunContext;
		private readonly int _requestCost;
		private readonly ActionPolicyAsync _throttlerActionPolicy;
		private const string CallerType = nameof(ThrottlerAsync);

		public ThrottlerAsync( ThrottlerConfig config, SyncRunContext syncRunContext )
		{
			this._maxQuota = config._maxQuota;
			var maxRetryCount = config._maxRetryCount;
			this._accountId = config._accountId;
			this._syncRunContext = syncRunContext;
			this._requestCost = config._requestCost;
			var delayOnThrottlingException = config._delayOnThrottlingException;

			this._throttlerActionPolicy = ActionPolicyAsync.Handle<Exception>().RetryAsync( maxRetryCount, async (ex, i) =>
			{
				if ( this.IsExceptionFromThrottling( ex ) )
				{
					LightspeedLogger.Debug( _syncRunContext, CallerType, "Throttler: got throttling exception. Retrying..." );
					await delayOnThrottlingException();
				}
				else
				{
					var errMessage = $"Throttler: faced non-throttling exception: {ex.Message}";
					LightspeedLogger.Debug( _syncRunContext, CallerType, errMessage );

					if( !( ex is WebException webException ) ) throw new LightspeedException( errMessage, ex );
					if( !( webException.Response is HttpWebResponse response ) ) throw new LightspeedException( errMessage, ex );
					if (response.StatusCode == HttpStatusCode.Unauthorized)
					{
						throw ex;
					}

					try
					{
						var responseText = this.SetResponseText(response, errMessage);
						throw new LightspeedException(responseText, ex);
					}
					catch
					{
						throw new LightspeedException(errMessage, ex);
					}
				}
			});
		}

		// default throttler that implements Lightspeed leaky bucket
		public ThrottlerAsync( long accountId, SyncRunContext syncRunContext ): this( ThrottlerConfig.CreateDefault( accountId ), syncRunContext )
		{
		}
 
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
				LightspeedLogger.Debug( _syncRunContext, CallerType, "Throttler: request executed successfully" );
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
			var webException = exception as WebException;
			var response = webException?.Response as HttpWebResponse;

			return response != null
                   && webException.Status == WebExceptionStatus.ProtocolError
                   && response.StatusCode == (HttpStatusCode)429;
        }

        private string SetResponseText(HttpWebResponse response, string errMessage)
        {
            string responseText;

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseText = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(responseText))
            {
                responseText = errMessage;
            }

            return responseText;
        }

		private ThrottlingInfoItem GetRemainingQuota()
		{
			if( !LightspeedGlobalThrottlingInfo.GetThrottlingInfo( this._accountId, out var info ) )
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
			LightspeedLogger.Debug( _syncRunContext, CallerType,
				$"Current quota remaining for account {this._accountId} is: {remainingQuota.RemainingQuantity}" );

			if( remainingQuota.RemainingQuantity > this._requestCost )
			{
				// we set new remaining quota for case potential error (this is strange, but it worked so long time)
				remainingQuota = new ThrottlingInfoItem( remainingQuota.RemainingQuantity - this._requestCost, remainingQuota.DripRate );
				this.SetRemainingQuota( remainingQuota.RemainingQuantity > 0 ? remainingQuota.RemainingQuantity : 0, remainingQuota.DripRate );
				return;
			}

			var secondsForDelay = Convert.ToInt32( Math.Ceiling( ( this._requestCost - remainingQuota.RemainingQuantity ) / remainingQuota.DripRate ) );
            var millisecondsForDelay = secondsForDelay * 1000;

			LightspeedLogger.Debug( _syncRunContext, CallerType,
				$"Throttler: quota exceeded. Waiting {secondsForDelay} seconds..." );
			await Task.Delay( millisecondsForDelay );
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Throttler: Resuming..." );
		}

		private void SubtractQuota< TResult >( TResult result )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, 
				"Throttler: trying to get leaky bucket metadata from response" );

			if( QuotaParser.TryParseQuota( result, out var bucketMetadata ) )
			{
				LightspeedLogger.Debug( _syncRunContext, CallerType,
					$"Throttler: parsed leaky bucket metadata from response. Bucket size: {bucketMetadata.quotaSize}. Used: {bucketMetadata.quotaUsed}. Drip rate: {bucketMetadata.dripRate}" );
				var quotaDelta = bucketMetadata.quotaSize - bucketMetadata.quotaUsed;
				this.SetRemainingQuota( quotaDelta > 0 ? quotaDelta : 0, bucketMetadata.dripRate );
			}

			var remainingQuota = this.GetRemainingQuota();
			LightspeedLogger.Debug( _syncRunContext, CallerType,
				$"Throttler: subtracted quota, now available {remainingQuota.RemainingQuantity}, drip rate {remainingQuota.DripRate}" );
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