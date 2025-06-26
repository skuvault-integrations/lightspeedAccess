using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Misc
{
	public class ThrottlerConfig
	{
		public readonly ThrottlingInfoItem _maxQuota;
		public readonly Func< Task > _delay;
		public readonly int _maxRetryCount;
		public readonly long _accountId;
		// request cost in quota units, 1 unit for read request, 10 units for write request, according to LS devs
		public readonly int _requestCost;
		public readonly Func< Task > _delayOnThrottlingException;

		private static ThrottlerConfigBuilder GetDefaultBuilder( long accountId )
		{
			var builder = new ThrottlerConfigBuilder( accountId );
			builder
				.SetDelayFunc( () => Task.Delay( 60 * 1000 ) )
				.SetDelayFuncOnThrottlingExceptions( () => Task.Delay( 180 * 1000 ) )
				.SetMaxQuota( LightspeedThrottlingDefaults.LightspeedBucketSize, LightspeedThrottlingDefaults.LightspeedDripRate )
				.SetMaxRetryCount( 40 )
				.SetRequestCost( LightspeedThrottlingDefaults.ReadRequestCost );

			return builder;
		}

		public static ThrottlerConfig CreateDefault( long accountId )
		{
			var builder = GetDefaultBuilder( accountId );
			return builder.Build();
		}

		public static ThrottlerConfig CreateDefaultForWriteRequests( long accountId )
		{
			var builder = GetDefaultBuilder( accountId ).SetRequestCost( LightspeedThrottlingDefaults.WriteRequestCost );
			return builder.Build();
		}

		public ThrottlerConfig( ThrottlingInfoItem maxQuota, int maxRetryCount, Func<Task> delay, long accountId, int requestCost, Func< Task > delayOnThrottlingException )
		{
			this._maxQuota = maxQuota;
			this._maxRetryCount = maxRetryCount;
			this._delay = delay;
			this._accountId = accountId;
			this._requestCost = requestCost;
			this._delayOnThrottlingException = delayOnThrottlingException;
		}
	}
}
