using SkuVault.Lightspeed.Access.Models.Account;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Integrations.Core.Common;
using SkuVault.Integrations.Core.Logging;

namespace SkuVault.Lightspeed.Access
{
	internal class AccountService: LightspeedBaseService, IAccountService
	{
		private const string CallerType = nameof(AccountService);

		public AccountService( LightspeedConfig config, SyncRunContext syncRunContext, IIntegrationLogger logger ) :
			base( config, syncRunContext, logger )
		{
		}

		public int GetAccountId()
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var request = new GetAccountRequest();
			var result = this._webRequestServices.GetResponse< LightspeedAccountInfo >( request, _syncRunContext ).AccountId;

			_logger.LogOperationEnd( _syncRunContext, CallerType );

			return result;
		}
	}
}