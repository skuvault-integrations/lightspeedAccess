using System.Linq;
using lightspeedAccess.Models.Account;
using lightspeedAccess.Models.Request;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Services;

namespace lightspeedAccess
{
	internal class AccountService: IAccountService
	{
		private readonly WebRequestService _webRequestServices;

		public AccountService( LightspeedConfig config, LightspeedAuthService authService )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedAccountService with config {0}", config.ToString() );
			this._webRequestServices = new WebRequestService( config, null, authService );
		}

		public int GetAccoundId()
		{
			LightspeedLogger.Log.Debug( "Started getting account Id for current session" );
			var request = new GetAccountRequest();
			return this._webRequestServices.GetResponse< LightspeedAccountList >( request ).Account.First().AccountId;
		}
	}
}