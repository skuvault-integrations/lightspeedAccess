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
		{			var retryCount = 0;
			while( true )
			{
				var shouldWait = false;
				try
				{
					LightspeedLogger.Log.Warn( "Throttler: trying execute request for the {0} time", retryCount );
					return await this.TryExecuteAsync( funcToThrottle );
				}
				catch( Exception ex )
				{
					if( !this.IsExceptionFromThrottling( ex ) )
						throw;
					
					if( retryCount >= this._maxRetryCount )
						throw new ThrottlerException( "Throttle max retry count reached", ex );

					Console.WriteLine( "THROTTLER HIT!!!" );
					LightspeedLogger.Log.Warn( "Throttler: got throttling exception. Retrying..." );
					this._requestTimer.Restart();
					shouldWait = true;
					retryCount++;
					// try again through loop
				}
				
				if ( shouldWait )
				{
					await this._delay();
				}
					
			}
		}

		private async Task< TResult > TryExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			var mySemaphore = LightspeedGlobalThrottlingInfo.GetSemaphoreSync( this._accountId );
			await mySemaphore.WaitAsync();

			await this.WaitIfNeededAsync();
			
			TResult result = default(TResult);
			try
			{
				result = await funcToThrottle();
				LightspeedLogger.Log.Warn( "Throttler: request executed successfully" );
				this.SubtractQuota( result, mySemaphore );
			}
			catch
			{
				throw;
			}
			finally
			{
				mySemaphore.Release();
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
//			await this.semaphore.WaitAsync();
			try
			{
//				if( this._calculationType == QuotaCalculationType.Manual ) this.UpdateRequestQuoteFromTimer();
//				while( true )
//				{					
					var x = this.GetRemainingQuota();
//					if ( x < 10 )
//					{
//						Console.WriteLine( "Low quota, waiting... ( was {0})", x );
//						await this._delay();
//						var y = this.GetRemainingQuota();
//						Console.WriteLine( "Resuming ( now avail {0})...", y );
//						//						continue;
//						//						this.SetRemainingQuota( 180 );
//					}
//					else
//					{
//						x--;
//						this.SetRemainingQuota( x );	 
//					}
				Console.WriteLine( "Quota is: {0}", x );
					if ( x > 20 )
					{
						x--;
						this.SetRemainingQuota( x );
						return;
					}
					
//					return;
//				}
			}
			finally
			{
//				this.semaphore.Release();				
			}
			Console.WriteLine( "Low quota. Waiting..." );
			LightspeedLogger.Log.Warn( "Throttler: quota exceeded. Waiting..." );
			await this._delay();
			Console.WriteLine( "Resuming..." );
		}

		private async void SubtractQuota< TResult >( TResult result, SemaphoreSlim mySemaphore )
		{
//			await this.semaphore.WaitAsync();
			try
			{
				ResponseLeakyBucketMetadata bucketMetadata;
				LightspeedLogger.Log.Warn( "Throttler: trying to get leaky bucket metadata from response" );
				if ( QuotaParser.TryParseQuota( result, out bucketMetadata ) )
				{
					LightspeedLogger.Log.Warn( "Throttler: parsed leaky bucket metadata from response. Bucket size: {0}. Used: {1}", bucketMetadata.quotaSize, bucketMetadata.quotaUsed );
					this.SetRemainingQuota( bucketMetadata.quotaSize - bucketMetadata.quotaUsed );
					this._calculationType = QuotaCalculationType.FromServer;
//					if ( mySemaphore.CurrentCount == 0 )
//					{
//						var rel = bucketMetadata.quotaSize - bucketMetadata.quotaUsed - 1;
//						mySemaphore.Release( rel / 2 );
//						Console.WriteLine( "Additionaly released: {0}", rel / 2);
//					}
				}
				else
				{
					throw new ArgumentException( "oblom!!" );
					//					LightspeedLogger.Log.Warn( "Throttler: cannot parse leaky bucket metadata from response, using built-in quota calculation instead" );
					//					int remainingQuota = this.GetRemainingQuota();
					//					remainingQuota--;
					//					if ( remainingQuota < 0 )
					//						remainingQuota = 0;
					//					this.SetRemainingQuota( remainingQuota );
					//					this._calculationType = QuotaCalculationType.Manual;
				}
			}
			catch (Exception e )
			{
				Console.WriteLine( "WTF? {0}", e.Message );
			}
			finally
			{
//				this.semaphore.Release();
			}

			this._requestTimer.Start();
			LightspeedLogger.Log.Warn( "Throttler: substracted quota, now available {0}", this.GetRemainingQuota() );
		}

		private void UpdateRequestQuoteFromTimer()
		{
			if( !this._requestTimer.IsRunning || this.GetRemainingQuota() == this._maxQuota )
				return;

			var totalSeconds = this._requestTimer.Elapsed.TotalSeconds;
			var elapsed = ( int )Math.Floor( totalSeconds );

			var quotaReleased = this._releasedQuotaCalculator( elapsed );

			LightspeedLogger.Log.Warn( "Throttler: {0} seconds elapsed, quota released: {1}", elapsed, quotaReleased );

			if( quotaReleased == 0 )
				return;

			int remainingQuota = this.GetRemainingQuota();

			remainingQuota = Math.Min( remainingQuota + quotaReleased, this._maxQuota );
			this.SetRemainingQuota( remainingQuota );
			LightspeedLogger.Log.Warn( "Throttler: added quota, now available {0}", remainingQuota );

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