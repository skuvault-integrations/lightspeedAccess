using SkuVault.Integrations.Core.Common;
using SkuVault.Integrations.Core.Logging;
using SkuVault.Lightspeed.Access.Models.Configuration;

namespace SkuVault.Lightspeed.Access
{
	public interface ILightspeedFactory
	{
		ILightspeedOrdersService CreateOrdersService( LightspeedConfig config, SyncRunContext syncRunContext );
		ILightspeedShopService CreateShopsService( LightspeedConfig config, SyncRunContext syncRunContext );
		ILightspeedProductsService CreateProductsService( LightspeedConfig config, SyncRunContext syncRunContext );
		IAccountService CreateAccountsService( LightspeedConfig config, SyncRunContext syncRunContext );
		ILigthspeedAuthService CreateLightspeedAuthService( LightspeedConfig config, SyncRunContext syncRunContext );
	}

	public sealed class LightspeedFactory: ILightspeedFactory
	{

		private readonly IIntegrationLogger _logger;

		public LightspeedFactory( IIntegrationLogger logger )
		{
			_logger = logger;
		}

		public ILightspeedOrdersService CreateOrdersService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedOrdersService( config, syncRunContext, _logger );
		}

		public ILightspeedShopService CreateShopsService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedShopService( config, syncRunContext, _logger );
		}

		public ILightspeedProductsService CreateProductsService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedProductsService( config, syncRunContext, _logger );
		}

		public IAccountService CreateAccountsService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new AccountService( config, syncRunContext, _logger );
		}

		public ILigthspeedAuthService CreateLightspeedAuthService( LightspeedConfig config, SyncRunContext syncRunContext )
		{
			return new LightspeedAuthService( config.ClientId, config.ClientSecret, syncRunContext, _logger );
		}
	}
}