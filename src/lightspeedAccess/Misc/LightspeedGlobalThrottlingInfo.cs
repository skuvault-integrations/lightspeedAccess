using System.Collections.Concurrent;

namespace lightspeedAccess.Misc
{
	internal static class LightspeedGlobalThrottlingInfo
	{
		private static readonly ConcurrentDictionary< long, int > _throttlingInfo = new ConcurrentDictionary< long, int >();

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
