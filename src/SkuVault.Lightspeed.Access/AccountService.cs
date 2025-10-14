using System.Linq;
using SkuVault.Lightspeed.Access.Misc;
using SkuVault.Lightspeed.Access.Models.Account;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Lightspeed.Access.Services;
using SkuVault.Integrations.Core.Common;

namespace SkuVault.Lightspeed.Access
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
			LightspeedLogger.Info( syncRunContext, nameof(AccountService), "Started getting account Id for current session" );
			var request = new GetAccountRequest();
			return this._webRequestServices.GetResponse< LightspeedAccountList >( request, syncRunContext ).Account.First().AccountId;
		}
	}
}