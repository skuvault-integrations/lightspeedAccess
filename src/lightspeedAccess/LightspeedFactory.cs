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

		public ILightspeedOrdersService CreateOrdersService( LightspeedConfig config )
		{
			return new LightspeedOrdersService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ) );
		}

		public ILightspeedShopService CreateShopsService( LightspeedConfig config )
		{
			return new LightspeedShopService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ) );
		}

		public ILightspeedProductsService CreateProductsService( LightspeedConfig config )
		{
			return new LightspeedProductsService( config, new LightspeedAuthService( this.LightspeedClientId, this.LightspeedClientSecret ) );
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