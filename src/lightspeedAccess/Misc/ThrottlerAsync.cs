using System;
using System.Net;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Common;
using Netco.ActionPolicyServices;
using System.IO;
using System.Runtime.ExceptionServices;

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
	    private readonly ActionPolicyAsync _throttlerActionPolicy;

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
                if (this.IsExceptionFromThrottling(ex))
                {
                    LightspeedLogger.Debug("Throttler: got throttling exception. Retrying...", (int)this._accountId);
                    await this._delayOnThrottlingException();
                }
				else
				{
					var errMessage = $"Throttler: faced non-throttling exception: {ex.Message}";
					LightspeedLogger.Debug( errMessage, (int)this._accountId );

				    if( ex is WebException webException && webException.Response is HttpWebResponse response)
					{
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw ex;
                        }

					    try
                        {
                            string responseText = this.SetResponseText(response, errMessage);

                            throw new LightspeedException(responseText, ex);

                        }
                        catch
                        {
                            throw new LightspeedException(errMessage, ex);
                        }

					}

					throw new LightspeedException( errMessage, ex);
				}
			});
		}

		// default throttler that implements Lightspeed leaky bucket
		public ThrottlerAsync( long accountId ): this( ThrottlerConfig.CreateDefault( accountId ) )
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
            return exception is WebException webException
                   && webException.Status == WebExceptionStatus.ProtocolError
                   && webException.Response is HttpWebResponse response
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
			if( !LightspeedGlobalThrottlingInfo.GetThrottlingInfo( this._accountId, out ThrottlingInfoItem info ) )
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
			LightspeedLogger.Debug( $"Current quota remaining for account {this._accountId} is: {remainingQuota.RemainingQuantity}", (int)this._accountId );

			if( remainingQuota.RemainingQuantity > this._requestCost )
			{
				// we set new remaining quota for case potential error (this is strange, but it worked so long time)
				remainingQuota = new ThrottlingInfoItem( remainingQuota.RemainingQuantity - this._requestCost, remainingQuota.DripRate );
				this.SetRemainingQuota( remainingQuota.RemainingQuantity > 0 ? remainingQuota.RemainingQuantity : 0, remainingQuota.DripRate );
				return;
			}

			var secondsForDelay = Convert.ToInt32( Math.Ceiling( ( this._requestCost - remainingQuota.RemainingQuantity ) / remainingQuota.DripRate ) );
            var millisecondsForDelay = secondsForDelay * 1000;

			LightspeedLogger.Debug( $"Throttler: quota exceeded. Waiting {secondsForDelay} seconds...", ( int )this._accountId );
			await Task.Delay(millisecondsForDelay);
			LightspeedLogger.Debug( "Throttler: Resuming...", (int)this._accountId );			
		}

		private void SubtractQuota< TResult >( TResult result )
		{
			LightspeedLogger.Debug( "Throttler: trying to get leaky bucket metadata from response", ( int )this._accountId );
			if( QuotaParser.TryParseQuota( result, out ResponseLeakyBucketMetadata bucketMetadata ) )
			{
				LightspeedLogger.Debug( $"Throttler: parsed leaky bucket metadata from response. Bucket size: {bucketMetadata.quotaSize}. Used: {bucketMetadata.quotaUsed}. Drip rate: {bucketMetadata.dripRate}", ( int )this._accountId );
				var quotaDelta = bucketMetadata.quotaSize - bucketMetadata.quotaUsed;
				this.SetRemainingQuota( quotaDelta > 0 ? quotaDelta : 0, bucketMetadata.dripRate );
			}

			var remainingQuota = this.GetRemainingQuota();
			LightspeedLogger.Debug( $"Throttler: subtracted quota, now available {remainingQuota.RemainingQuantity}, drip rate {remainingQuota.DripRate}", ( int )this._accountId );
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