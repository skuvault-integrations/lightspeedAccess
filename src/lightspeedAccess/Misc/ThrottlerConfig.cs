using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Misc
{
	public class ThrottlerConfig
	{
		public readonly int _maxQuota;
		public readonly Func< Task > _delay;
		public readonly int _maxRetryCount;
		public readonly long _accountId;

		public static ThrottlerConfig CreateDefault( long accountId )
		{
			var builder = new ThrottlerConfigBuilder( accountId );
			builder
				.SetDelayFunc( () => Task.Delay( 60 * 1000 ) )
				.SetMaxQuota( LightspeedThrottlingDefaults.LightspeedBucketSize )
				.SetMaxRetryCount( 20 )
				.Build();

			return builder.Build();
		}

		public ThrottlerConfig( int maxQuota, int maxRetryCount, Func< Task > delay, long accountId )
		{
			this._maxQuota = maxQuota;
			this._maxRetryCount = maxRetryCount;
			this._delay = delay;
			this._accountId = accountId;
		}
	}
}
