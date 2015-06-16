using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess;
using LightspeedAccess.Models.Configuration;

namespace LightspeedAccess
{
	public interface ILightspeedFactory
	{
		ILightspeedOrdersService CreateOrdersService( LightspeedConfig config );
		ILightspeedShopService CreateShopsService( LightspeedConfig config );
		IAccountService CreateAccountsService( LightspeedConfig config );
		ILigthspeedAuthService CreateLightspeedAuthService();
	}

	public sealed class LightspeedFactory : ILightspeedFactory
	{
		private string LightspeedClientId { get; set; }
		private string LightspeedClientSecret { get; set; }
		private string LightspeedRedirectUri { get; set; }

		public LightspeedFactory(string clientId, string clientSecret, string redirectUri)
		{
			LightspeedClientId = clientId;
			LightspeedClientSecret = clientSecret;
			LightspeedRedirectUri = redirectUri;
		}

		public ILightspeedOrdersService CreateOrdersService(LightspeedConfig config)
		{
			return new LightspeedOrdersService(config);
		}

		public ILightspeedShopService CreateShopsService( LightspeedConfig config )
		{
			return new LightspeedShopService( config );
		}

		public IAccountService CreateAccountsService( LightspeedConfig config )
		{
			return new AccountService( config );
		}

		public ILigthspeedAuthService CreateLightspeedAuthService()
		{
			return new LightspeedAuthService( LightspeedClientId, LightspeedClientSecret, LightspeedRedirectUri );
		}
	}
}
