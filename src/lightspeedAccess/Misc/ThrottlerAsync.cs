using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models;
using lightspeedAccess.Models.Common;
using Netco.ActionPolicyServices;

namespace LightspeedAccess.Misc
{
	public sealed class ThrottlerAsync
	{
		private readonly int _maxQuota;
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
				if ( !this.IsExceptionFromThrottling( ex ) )
				{
					LightspeedLogger.Log.Debug( "Throttler: faced non-throttling exception: {0}", ex.Message );
					throw ex;
				}
				LightspeedLogger.Log.Debug( "Throttler: got throttling exception. Retrying..." );
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
			return await this._throttlerActionPolicy.Get( () => this.TryExecuteAsync( funcToThrottle ) );
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
				LightspeedLogger.Log.Debug( "Throttler: request executed successfully" );
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

		private int GetRemainingQuota()
		{
			int remainingQuota;
			if( !LightspeedGlobalThrottlingInfo.GetThrottlingInfo( this._accountId, out remainingQuota ) )
				remainingQuota = this._maxQuota;
			return remainingQuota;
		}

		private void SetRemainingQuota( int quota )
		{
			LightspeedGlobalThrottlingInfo.AddThrottlingInfo( this._accountId, quota );
		}

		private async Task WaitIfNeededAsync()
		{
			var remainingQuota = this.GetRemainingQuota();
			LightspeedLogger.Log.Debug( "Current quota for account {0} is: {1}", this._accountId, remainingQuota );
			if( remainingQuota > QuotaThreshold )
			{
				remainingQuota = remainingQuota - this._requestCost;
				this.SetRemainingQuota( remainingQuota > 0 ? remainingQuota : 0 );
				return;
			}

			LightspeedLogger.Log.Debug( "Throttler: quota exceeded. Waiting..." );
			await this._delay();
			LightspeedLogger.Log.Debug( "Throttler: Resuming..." );			
		}

		private void SubtractQuota< TResult >( TResult result )
		{
			ResponseLeakyBucketMetadata bucketMetadata;
			LightspeedLogger.Log.Debug( "Throttler: trying to get leaky bucket metadata from response" );
			if( QuotaParser.TryParseQuota( result, out bucketMetadata ) )
			{
				LightspeedLogger.Log.Debug( "Throttler: parsed leaky bucket metadata from response. Bucket size: {0}. Used: {1}", bucketMetadata.quotaSize, bucketMetadata.quotaUsed );
				var quotaDelta = bucketMetadata.quotaSize - bucketMetadata.quotaUsed;
				this.SetRemainingQuota( quotaDelta > 0 ? quotaDelta : 0 );
			}

			LightspeedLogger.Log.Debug( "Throttler: substracted quota, now available {0}", this.GetRemainingQuota() );
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