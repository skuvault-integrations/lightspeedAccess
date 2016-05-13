using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models;
using lightspeedAccess.Models.Common;

namespace LightspeedAccess.Misc
{
	public sealed class ThrottlerAsync
	{
		private readonly int _maxQuota;
		private readonly long _accountId;
		private QuotaCalculationType _calculationType = QuotaCalculationType.FromServer; 
		private readonly Func< int, int > _releasedQuotaCalculator;
		private readonly Func< Task > _delay;
		private readonly int _maxRetryCount;

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim( 1 );

		public ThrottlerAsync( ThrottlerConfig config )
		{
			this._maxQuota = config._maxQuota;
			this._releasedQuotaCalculator = config._releasedQuotaCalculator;
			this._delay = config._delay;
			this._maxRetryCount = config._maxRetryCount;
			this._accountId = config._accountId;
		}


		// default throttler that implements Lightspeed leaky bucket
		public ThrottlerAsync( long accountId ): this( ThrottlerConfig.CreateDefault( accountId ) )
		{
		}

		public async Task< TResult > ExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			var retryCount = 0;
			while( true )
			{
				var shouldWait = false;
				try
				{
					LightspeedLogger.Log.Debug( "Throttler: trying execute request for the {0} time", retryCount );
					return await this.TryExecuteAsync( funcToThrottle );
				}
				catch( Exception ex )
				{
					if( !this.IsExceptionFromThrottling( ex ) )
						throw;

					if( retryCount >= this._maxRetryCount )
						throw new ThrottlerException( "Throttle max retry count reached", ex );

					LightspeedLogger.Log.Debug( "Throttler: got throttling exception. Retrying..." );
					this.SetRemainingQuota( 0 );
					this._requestTimer.Restart();
					shouldWait = true;
					retryCount++;
					// try again through loop
				}
				if ( shouldWait )
				{
					LightspeedLogger.Log.Debug( "Throttler: waiting before next retry..." );
					await this._delay();
				}
					
			}
		}

		private async Task< TResult > TryExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			await this.WaitIfNeededAsync();
			var result = await funcToThrottle();
			LightspeedLogger.Log.Debug( "Throttler: request executed successfully" );
			this.SubtractQuota( result );
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
			await this.semaphore.WaitAsync();
			try
			{
				if( this._calculationType == QuotaCalculationType.Manual ) this.UpdateRequestQuoteFromTimer();

				if ( this.GetRemainingQuota() != 0 )
					return;
			}
			finally
			{
				this.semaphore.Release();				
			}

			LightspeedLogger.Log.Debug( "Throttler: quota exceeded. Waiting..." );
			await this._delay();
		}

		private async void SubtractQuota< TResult >( TResult result )
		{
			await this.semaphore.WaitAsync();
			try
			{
				ResponseLeakyBucketMetadata bucketMetadata;
				LightspeedLogger.Log.Debug( "Throttler: trying to get leaky bucket metadata from response" );
				if ( QuotaParser.TryParseQuota( result, out bucketMetadata ) )
				{
					LightspeedLogger.Log.Debug( "Throttler: parsed leaky bucket metadata from response. Bucket size: {0}. Used: {1}", bucketMetadata.quotaSize, bucketMetadata.quotaUsed );
					this.SetRemainingQuota( bucketMetadata.quotaSize - bucketMetadata.quotaUsed );
					this._calculationType = QuotaCalculationType.FromServer;
				}
				else
				{
					LightspeedLogger.Log.Debug( "Throttler: cannot parse leaky bucket metadata from response, using built-in quota calculation instead" );
					int remainingQuota = this.GetRemainingQuota();
					remainingQuota--;
					if ( remainingQuota < 0 )
						remainingQuota = 0;
					this.SetRemainingQuota( remainingQuota );
					this._calculationType = QuotaCalculationType.Manual;
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			this._requestTimer.Start();
			LightspeedLogger.Log.Debug( "Throttler: substracted quota, now available {0}", this.GetRemainingQuota() );
		}

		private void UpdateRequestQuoteFromTimer()
		{
			if( !this._requestTimer.IsRunning || this.GetRemainingQuota() == this._maxQuota )
				return;

			var totalSeconds = this._requestTimer.Elapsed.TotalSeconds;
			var elapsed = ( int )Math.Floor( totalSeconds );

			var quotaReleased = this._releasedQuotaCalculator( elapsed );

			LightspeedLogger.Log.Debug( "Throttler: {0} seconds elapsed, quota released: {1}", elapsed, quotaReleased );

			if( quotaReleased == 0 )
				return;

			int remainingQuota = this.GetRemainingQuota();

			remainingQuota = Math.Min( remainingQuota + quotaReleased, this._maxQuota );
			this.SetRemainingQuota( remainingQuota );
			LightspeedLogger.Log.Debug( "Throttler: added quota, now available {0}", remainingQuota );

			this._requestTimer.Reset();
		}

		private readonly Stopwatch _requestTimer = new Stopwatch();

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
	}
}