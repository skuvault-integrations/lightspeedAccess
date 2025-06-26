using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Models.Configuration;

namespace SkuVault.Lightspeed.Access
{
	public interface ILightspeedFactory
	{
		ILightspeedOrdersService CreateOrdersService( LightspeedConfig config, SyncRunContext syncRunContext );
		ILightspeedShopService CreateShopsService( LightspeedConfig config, SyncRunContext syncRunContext );
		ILightspeedProductsService CreateProductsService( LightspeedConfig config, SyncRunContext syncRunContext );
		IAccountService CreateAccountsService( LightspeedConfig config );
		ILigthspeedAuthService CreateLightspeedAuthService();
	}

	public sealed class LightspeedFactory: ILightspeedFactory
	{
		private string LightspeedClientId{ get; set; }
		private string LightspeedClientSecret{ get; set; }
		private string LightspeedRedirectUri{ get; set; }

		public LightspeedFactory( string clientId, string clientSecret, string redirectUri )
		{
			this.LightspeedClientId = clientId;
			this.LightspeedClientSecret = clientSecret;
			this.LightspeedRedirectUri = redirectUri;
		}

		public ILightspeedOrdersService CreateOrdersService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedOrdersService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ), syncRunContext );
		}

		public ILightspeedShopService CreateShopsService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedShopService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ), syncRunContext );
		}

		public ILightspeedProductsService CreateProductsService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedProductsService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ), syncRunContext );
		}
		
		public IAccountService CreateAccountsService( LightspeedConfig config )
		{
			return new AccountService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ) );
		}

		public ILigthspeedAuthService CreateLightspeedAuthService()
		{
			return new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret );
		}
	}
}