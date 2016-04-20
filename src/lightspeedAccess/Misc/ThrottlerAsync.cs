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
		private int _remainingQuota;
		private readonly Func< int, int > _releasedQuotaCalculator;
		private readonly Func< Task > _delay;
		private readonly int _maxRetryCount;

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim( 1 );

		//TODO: Update delayInSeconds to milliseconds or change type to decimal
		public ThrottlerAsync( int maxQuota, int delayInSeconds ):
			this( maxQuota, el => el / delayInSeconds, () => Task.Delay( delayInSeconds * 1000 ), 10 )
		{
		}

		public ThrottlerAsync( int maxQuota, Func< int, int > releasedQuotaCalculator, Func< Task > delay, int maxRetryCount )
		{
			this._maxQuota = this._remainingQuota = maxQuota;
			this._releasedQuotaCalculator = releasedQuotaCalculator;
			this._delay = delay;
			this._maxRetryCount = maxRetryCount;
		}

		private const int LightspeedBucketSize = 180;
		private const int LightspeedDripRate = 3;

		// default throttler that implements Lightspeed leaky bucket
		public ThrottlerAsync(): this( LightspeedBucketSize, elapsedTimeSeconds => elapsedTimeSeconds * LightspeedDripRate, () => Task.Delay( 60 * 1000 ), 20 )
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
					this._remainingQuota = 0;
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

		private async Task WaitIfNeededAsync()
		{
			await this.semaphore.WaitAsync();
			try
			{
				this.UpdateRequestQuoteFromTimer();

				if ( this._remainingQuota != 0 )
				{
					return;
				}
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
					this._remainingQuota = bucketMetadata.quotaSize - bucketMetadata.quotaUsed;
				}
				else
				{
					LightspeedLogger.Log.Debug( "Throttler: cannot parse leaky bucket metadata from response, using built-in quota calculation instead" );
					this._remainingQuota--;
					if ( this._remainingQuota < 0 )
						this._remainingQuota = 0;
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			this._requestTimer.Start();
			LightspeedLogger.Log.Debug( "Throttler: substracted quota, now available {0}", this._remainingQuota );
		}

		private void UpdateRequestQuoteFromTimer()
		{
			if( !this._requestTimer.IsRunning || this._remainingQuota == this._maxQuota )
				return;

			var totalSeconds = this._requestTimer.Elapsed.TotalSeconds;
			var elapsed = ( int )Math.Floor( totalSeconds );

			var quotaReleased = this._releasedQuotaCalculator( elapsed );

			LightspeedLogger.Log.Debug( "Throttler: {0} seconds elapsed, quota released: {1}", elapsed, quotaReleased );

			if( quotaReleased == 0 )
				return;

			this._remainingQuota = Math.Min( this._remainingQuota + quotaReleased, this._maxQuota );
			LightspeedLogger.Log.Debug( "Throttler: added quota, now available {0}", this._remainingQuota );

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