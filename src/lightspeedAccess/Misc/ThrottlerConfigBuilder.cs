using System;
using System.Threading.Tasks;

namespace lightspeedAccess.Misc
{
	class ThrottlerConfigBuilder
	{
		private ThrottlingInfoItem _maxQuota;
		private Func<Task> _delay;
		private int _maxRetryCount;
		private readonly long _accountId;
		private int _requestCost;
		public Func<Task> _delayOnThrottlingException;

		public ThrottlerConfigBuilder( long accountId)
		{
			this._accountId = accountId;
		}

		public ThrottlerConfigBuilder SetMaxQuota( int maxQuota, float dripRate )
		{
			this._maxQuota = new ThrottlingInfoItem( maxQuota, dripRate );
			return this;
		}

		public ThrottlerConfigBuilder SetMaxRetryCount( int maxRetryCount )
		{
			this._maxRetryCount = maxRetryCount;
			return this;
		}

		public ThrottlerConfigBuilder SetDelayFunc( Func< Task > delay )
		{
			this._delay = delay;
			return this;
		}

		public ThrottlerConfigBuilder SetDelaySeconds( int seconds )
		{
			this._delay = () => Task.Delay( seconds * 1000 );
			return this;
		}

		public ThrottlerConfigBuilder SetRequestCost( int requestCost )
		{
			this._requestCost = requestCost;
			return this;
		}

		public ThrottlerConfigBuilder SetDelayFuncOnThrottlingExceptions( Func< Task > delay )
		{
			this._delayOnThrottlingException = delay;
			return this;
		}

		public ThrottlerConfig Build()
		{
			return new ThrottlerConfig( this._maxQuota, this._maxRetryCount, this._delay, this._accountId, this._requestCost, this._delayOnThrottlingException );
		}
	}
}