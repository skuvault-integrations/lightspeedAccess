using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess.Models.Account;
using lightspeedAccess.Models.Request;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Services;

namespace lightspeedAccess
{
	class AccountService : IAccountService
	{
		private readonly WebRequestService _webRequestServices;

		public AccountService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedAccountService with config {0}", config.ToString() );	
			_webRequestServices = new WebRequestService( config );
		}

		public int GetAccoundId()
		{
			LightspeedLogger.Log.Debug( "Started getting account Id for current session" );
			var request = new GetAccountRequest();
			return this._webRequestServices.GetResponse<LightspeedAccountList>( request ).Account.First().AccountId;
		}
	}
}
