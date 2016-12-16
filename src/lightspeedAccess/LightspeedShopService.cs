using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.Shop;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Models.Shop;
using LightspeedAccess.Services;
using Netco.Extensions;

namespace LightspeedAccess
{
	public class LightspeedShopService: ILightspeedShopService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly WebRequestService _webRequestServicesForUpdating;
		private readonly LightspeedConfig _config;

		public LightspeedShopService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedShopsService with config {0}", config.ToString() );
			this._webRequestServices = new WebRequestService( config, new ThrottlerAsync( config.AccountId ) );
			this._webRequestServicesForUpdating = new WebRequestService( config, new ThrottlerAsync( ThrottlerConfig.CreateDefaultForWriteRequests( config.AccountId ) ) );
			this._config = config;
		}

		public IEnumerable< Shop > GetShops()
		{
			LightspeedLogger.Log.Debug( "Starting to get Shops" );
			var getShopsRequest = new GetShopRequest();
			var shops = this._webRequestServices.GetResponse< ShopsList >( getShopsRequest ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting to get Shops" );

			var getShopsRequest = new GetShopRequest();
			var shops = ( await this._webRequestServices.GetResponseAsync< ShopsList >( getShopsRequest, ctx ) ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public void UpdateOnHandQuantity( int itemId, string sku, int shopId, string shopName, int itemShopRelationId, int quantity )
		{
			string paramInfo = $"itemId:{itemId}, sku:\"{sku}\", shopId:{shopId}, shopName:\"{shopName}\", itemShopRelationId:{itemShopRelationId}, quantity:{quantity}";
			LightspeedLogger.Log.Debug( "Starting update shop item quantity. " + paramInfo );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			this._webRequestServicesForUpdating.GetResponse< LightspeedProduct >( updateOnHandQuantityRequest );
			LightspeedLogger.Log.Debug( "Quantity updated successfully. " + paramInfo );
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, string sku, int shopId, string shopName, int itemShopRelationId, int quantity, CancellationToken ctx )
		{
			string paramInfo = $"itemId:{itemId}, sku:\"{sku}\", shopId:{shopId}, shopName:\"{shopName}\", itemShopRelationId:{itemShopRelationId}, quantity:{quantity}";
			LightspeedLogger.Log.Debug( "Starting update shop item quantity. " + paramInfo );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			await this._webRequestServicesForUpdating.GetResponseAsync< LightspeedProduct >( updateOnHandQuantityRequest, ctx );
			LightspeedLogger.Log.Debug( "Quantity updated successfully. " + paramInfo );
		}

		public async Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkusFull, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting to get item sku index" );
			var itemSkusPartitioned = itemSkusFull.ToList().Partition( 100 );

			var dictionary = new Dictionary< string, LightspeedProduct >();

			var tasks = itemSkusPartitioned.Select( itemSkus =>
			{
				var getItemRequest = new GetItemsRequest( itemSkus );
				return this._webRequestServices.GetResponseAsync<LightspeedProductList>( getItemRequest, ctx );
			} );

			await Task.WhenAll( tasks );

			tasks.ForEach( t =>
			{
				if ( t.Result.Item != null )
				{
					LightspeedLogger.Log.Debug( "Got {0} entries in item sku index", t.Result.Item.Length );
					t.Result.Item.ToList().Distinct().ForEach( i =>
					{
						dictionary[ i.Sku ] = i;
					} );
				}
			} );
			return dictionary;
		}

		private async Task<IEnumerable<LightspeedProduct>> ExecuteGetItemsRequest( GetItemsRequest request, CancellationToken ctx ) {
			var result = new List<LightspeedProduct>();
			var response = await this._webRequestServices.GetResponseAsync<LightspeedProductList>( request, ctx );
			if ( response.Item != null )
				result = response.Item.ToList();
			return result;			
		} 

		public async Task< bool > DoesItemExist( int itemId, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Checking, if item {0} exists", itemId );
			var request = new GetItemRequest( itemId );
			var response = await this._webRequestServices.GetResponseAsync< LightspeedProduct >( request, ctx );
			return response != null;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItemsCreatedInShopAsync( int shopId, DateTime createTimeUtc, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Getting items, created in shop {0} after {1}", shopId, createTimeUtc );
			var getItemRequest = new GetItemsRequest( shopId, createTimeUtc );
			var result = await this.ExecuteGetItemsRequest( getItemRequest, ctx );
			LightspeedLogger.Log.Debug( "Getting {0} items updated after {1} in shop {2}", result.Count(), createTimeUtc, shopId );
			return result;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx )
		{
			var getItemRequest = new GetItemsRequest( shopId );
			return await this.ExecuteGetItemsRequest( getItemRequest, ctx );
		}

		public ShopOrder MakeOrderRequest< T >( string endpoint, string token, T body, string method ) where T : ShopOrderBase
		{
			var uri = new Uri( this._config.Endpoint + endpoint + "?oauth_token=" + token );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = method;
			var serializer = new XmlSerializer( typeof( T ) );
			Stream requestStream = new MemoryStream();

			serializer.Serialize( requestStream, body );

			request.ContentType = "text/xml";

			requestStream.Seek( 0, SeekOrigin.Begin );
			var sr = new StreamReader( requestStream );
			var s = sr.ReadToEnd();

			request.ContentLength = s.Length;
			Stream dataStream = request.GetRequestStream();

			for( var i = 0; i < s.Length; i++ )
			{
				dataStream.WriteByte( Convert.ToByte( s[ i ] ) );
			}
			dataStream.Close();

			var response = request.GetResponse();
			var stream = response.GetResponseStream();

			var deserializer =
				new XmlSerializer( typeof( ShopOrder ) );

			var order = ( ShopOrder )deserializer.Deserialize( stream );
			return order;
		}
	}
}