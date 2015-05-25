using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.Shop;
using lightspeedAccess.Services;

namespace lightspeedAccess
{
	class LightspeedShopService : ILightspeedShopService
	{

		private readonly WebRequestService _webRequestServices;

		public LightspeedShopService( LightspeedConfig config )
		{
			_webRequestServices = new WebRequestService( config );
		}

		public IEnumerable< Shop > GetShops()
		{
			// TODO use shop names
			var getShopsRequest = new GetShopRequest();
			return _webRequestServices.GetResponse<ShopsList>( getShopsRequest ).Shop;
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync(CancellationToken ctx)
		{
			var getShopsRequest = new GetShopRequest();
			return (await _webRequestServices.GetResponseAsync<ShopsList>( getShopsRequest, ctx )).Shop;
		}

		public void UpdateOnHandQuantity(int itemId, int shopId, int quantity)
		{
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest(itemId, shopId, quantity);
			_webRequestServices.GetResponse<LightspeedProduct>( updateOnHandQuantityRequest);
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, int shopId, int quantity, CancellationToken ctx )
		{
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, quantity );
			await _webRequestServices.GetResponseAsync<LightspeedProduct>( updateOnHandQuantityRequest, ctx );
		}

	}
}
