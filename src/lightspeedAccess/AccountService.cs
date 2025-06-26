using System.Linq;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Account;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Services;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccess
{
	internal class AccountService: IAccountService
	{
		private readonly WebRequestService _webRequestServices;

		public AccountService( LightspeedConfig config, LightspeedAuthService authService )
		{
			this._webRequestServices = new WebRequestService( config, null, authService );
		}

		public int GetAccountId( SyncRunContext syncRunContext )
		{
			LightspeedLogger.Debug( syncRunContext, nameof(AccountService), "Started getting account Id for current session" );
			var request = new GetAccountRequest();
			return this._webRequestServices.GetResponse< LightspeedAccountList >( request, syncRunContext ).Account.First().AccountId;
		}
	}
}