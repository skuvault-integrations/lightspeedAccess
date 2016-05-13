using System;
using System.Threading.Tasks;

namespace lightspeedAccess.Misc
{
	class ThrottlerConfigBuilder
	{
		private int _maxQuota;
		private Func<int, int> _releasedQuotaCalculator;
		private Func<Task> _delay;
		private int _maxRetryCount;
		private readonly long _accountId;				

		public ThrottlerConfigBuilder( long accountId)
		{
			this._accountId = accountId;
		}

		public ThrottlerConfigBuilder SetMaxQuota( int maxQuota )
		{
			this._maxQuota = maxQuota;
			return this;
		}

		public ThrottlerConfigBuilder SetMaxRetryCount( int maxRetryCount )
		{
			this._maxRetryCount = maxRetryCount;
			return this;
		}

		public ThrottlerConfigBuilder SetReleasedQuotaCalculator( Func<int, int> releasedQuotaCalculator )
		{
			this._releasedQuotaCalculator = releasedQuotaCalculator;
			return this;
		}

		public ThrottlerConfigBuilder SetDelayFunc( Func<Task> delay )
		{
			this._delay = delay;
			return this;
		}

		public ThrottlerConfigBuilder SetDelaySeconds( int seconds )
		{
			this._delay = () => Task.Delay( seconds * 1000 );
			return this;
		}

		public ThrottlerConfig Build()
		{
			return new ThrottlerConfig( this._maxQuota, this._maxRetryCount, this._delay, this._releasedQuotaCalculator, this._accountId );
		}
	}
}