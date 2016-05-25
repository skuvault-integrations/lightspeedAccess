using System.Collections.Concurrent;
using System.Threading;

namespace lightspeedAccess.Misc
{
	internal static class LightspeedGlobalThrottlingInfo
	{
		private const int MaxParallelRequestsForSingleAccount = 10;

		private static readonly ConcurrentDictionary< long, int > _throttlingInfo = new ConcurrentDictionary< long, int >();
		private static readonly ConcurrentDictionary< long, SemaphoreSlim > _semaphoreQuota = new ConcurrentDictionary< long, SemaphoreSlim >();

		public static SemaphoreSlim GetSemaphoreSync( long accountId )
		{
			if( _semaphoreQuota.ContainsKey( accountId ) )
				return _semaphoreQuota[ accountId ];
			var newSemaphore = new SemaphoreSlim( MaxParallelRequestsForSingleAccount );
			_semaphoreQuota[ accountId ] = newSemaphore;
			return newSemaphore;
		}

		public static void AddThrottlingInfo( long accountId, int remainingQuota )
		{
			_throttlingInfo[ accountId ] = remainingQuota;
		}

		public static bool GetThrottlingInfo( long accountId, out int quota )
		{
			return _throttlingInfo.TryGetValue( accountId, out quota );
		}
	}
}
