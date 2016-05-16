using System.Collections.Concurrent;
using System.Threading;

namespace lightspeedAccess.Misc
{
	internal static class LightspeedGlobalThrottlingInfo
	{
		
		private static readonly ConcurrentDictionary< long, int > _throttlingInfo = new ConcurrentDictionary< long, int >();
		private static readonly ConcurrentDictionary< long, SemaphoreSlim > semaphoreQuota = new ConcurrentDictionary< long, SemaphoreSlim >();

		public static readonly SemaphoreSlim GlobalThrottlerSwitch = new SemaphoreSlim( 1 );

		public static SemaphoreSlim GetSemaphoreSync( long accountId )
		{
			if( semaphoreQuota.ContainsKey( accountId ) )
				return semaphoreQuota[ accountId ];
			var x = new SemaphoreSlim( 10 );
			semaphoreQuota[ accountId ] = x;
			return x;
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
