using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Models.Shop;
using LightspeedAccess.Services;

namespace LightspeedAccess
{
	class LightspeedShopService : ILightspeedShopService
	{

		private readonly WebRequestService _webRequestServices;

		public LightspeedShopService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedShopsService with config {0}", config.ToString() );
			_webRequestServices = new WebRequestService( config );
		}

		public IEnumerable< Shop > GetShops()
		{
			// TODO use shop names
			LightspeedLogger.Log.Debug( "Starting to get Shops" );
			var getShopsRequest = new GetShopRequest();
			var shops = _webRequestServices.GetResponse<ShopsList>( getShopsRequest ).Shop;
			if (shops == null) return new List< Shop >();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync(CancellationToken ctx)
		{
			LightspeedLogger.Log.Debug( "Starting to get Shops" );

			var getShopsRequest = new GetShopRequest();
			var shops = ( await _webRequestServices.GetResponseAsync<ShopsList>( getShopsRequest, ctx ) ).Shop;
			if ( shops == null ) return new List<Shop>();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public void UpdateOnHandQuantity(int itemId, int shopId, int quantity)
		{
			LightspeedLogger.Log.Debug( "Starting update shop item quantity" );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest(itemId, shopId, quantity);
			_webRequestServices.GetResponse<LightspeedProduct>( updateOnHandQuantityRequest);
			LightspeedLogger.Log.Debug( "Quantity updated successfully" );
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, int shopId, int quantity, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting update shop item quantity" );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, quantity );
			await _webRequestServices.GetResponseAsync<LightspeedProduct>( updateOnHandQuantityRequest, ctx );
			LightspeedLogger.Log.Debug( "Quantity updated successfully" );
		}

		public async Task<IDictionary<string, int> > GetItems( IEnumerable<string> itemSkus, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting to get item sku index" );
			var getItemRequest = new GetItemsRequest( itemSkus );
			var result = await _webRequestServices.GetResponseAsync<LightspeedProductList>( getItemRequest, ctx );
			if ( result.Item != null )
			{
				LightspeedLogger.Log.Debug( "Got {0} entries in item sku index",  result.Item.Length );
				return result.Item.ToList().Distinct().Select( i => new Tuple<String, int>( i.Sku, i.ItemId ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			}
			return new Dictionary<string, int>();
		}

	}
}
