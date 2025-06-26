using System.Collections.Concurrent;
using System.Threading;

namespace lightspeedAccess.Misc
{
	internal static class LightspeedGlobalThrottlingInfo
	{
		private const int MaxParallelRequestsForSingleAccount = 10;

		private static readonly ConcurrentDictionary< long, ThrottlingInfoItem > _throttlingInfo = new ConcurrentDictionary< long, ThrottlingInfoItem >();
		private static readonly ConcurrentDictionary< long, SemaphoreSlim > _semaphoreQuota = new ConcurrentDictionary< long, SemaphoreSlim >();

		public static SemaphoreSlim GetSemaphoreSync( long accountId )
		{
			if( _semaphoreQuota.ContainsKey( accountId ) )
				return _semaphoreQuota[ accountId ];
			var newSemaphore = new SemaphoreSlim( MaxParallelRequestsForSingleAccount );
			_semaphoreQuota[ accountId ] = newSemaphore;
			return newSemaphore;
		}

		public static void AddThrottlingInfo( long accountId, ThrottlingInfoItem info )
		{
			_throttlingInfo[ accountId ] = info;
		}

		public static bool GetThrottlingInfo( long accountId, out ThrottlingInfoItem info )
		{
			return _throttlingInfo.TryGetValue( accountId, out info );
		}
	}

	public class ThrottlingInfoItem
	{
		public int RemainingQuantity{ get; set; }
		public float DripRate{ get; set; }

		public ThrottlingInfoItem( int remainingQuantity, float dripRate )
		{
			this.RemainingQuantity = remainingQuantity;
			this.DripRate = dripRate;
		}
	}
}
