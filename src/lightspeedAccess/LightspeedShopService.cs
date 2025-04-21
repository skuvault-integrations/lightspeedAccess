using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using lightspeedAccess;
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
using SkuVault.Integrations.Core.Common;

namespace LightspeedAccess
{
	public class LightspeedShopService: ILightspeedShopService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly WebRequestService _webRequestServicesForUpdating;
		private readonly LightspeedConfig _config;
		private readonly SyncRunContext _syncRunContext;
		private const string CallerType = nameof(LightspeedShopService);

		public LightspeedShopService( LightspeedConfig config, LightspeedAuthService authService, SyncRunContext syncRunContext )
		{
			this._webRequestServices = new WebRequestService( config, new ThrottlerAsync( config.AccountId, syncRunContext ), authService );
			this._webRequestServicesForUpdating = new WebRequestService( config, new ThrottlerAsync( ThrottlerConfig.CreateDefaultForWriteRequests( config.AccountId ), syncRunContext ), authService );
			this._config = config;
			this._syncRunContext = syncRunContext;
		}

		public IEnumerable< Shop > GetShops()
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Starting to get Shops" );

			var getShopsRequest = new GetShopRequest();
			var shops = this._webRequestServices.GetResponse< ShopsList >( getShopsRequest, _syncRunContext ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Debug( _syncRunContext, CallerType, $"Retrieved {shops.Length} shops" );
			return shops.ToList();
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Starting to get Shops" );

			var getShopsRequest = new GetShopRequest();
			var shops = ( await this._webRequestServices.GetResponseAsync< ShopsList >( getShopsRequest, _syncRunContext, ctx ) ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Debug( _syncRunContext, CallerType, $"Retrieved {shops.Length} shops" );
			return shops.ToList();
		}

		public void UpdateOnHandQuantity( int itemId, int shopId, int itemShopRelationId, int quantity, string logComment = null )
		{
			var paramInfo = string.Format( "itemId:{0}, shopId:{1}, itemShopRelationId:{2}, quantity:{3}{4}", 
				itemId, shopId, itemShopRelationId, quantity, ( !string.IsNullOrWhiteSpace( logComment ) ? ", " : "" ) + logComment );
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Starting update shop item quantity. " + paramInfo );

			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			this._webRequestServicesForUpdating.GetResponse< LightspeedProduct >( updateOnHandQuantityRequest, _syncRunContext );

			LightspeedLogger.Debug( _syncRunContext, CallerType, "Quantity updated successfully. " + paramInfo );
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, int shopId, int itemShopRelationId, int quantity, 
			CancellationToken ctx, string logComment = null )
		{
			var paramInfo = string.Format( "itemId:{0}, shopId:{1}, itemShopRelationId:{2}, quantity:{3}{4}",
				itemId, shopId, itemShopRelationId, quantity, ( !string.IsNullOrWhiteSpace( logComment ) ? ", " : "" ) + logComment );
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Starting update shop item quantity. " + paramInfo );

			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			await this._webRequestServicesForUpdating.GetResponseAsync< LightspeedProduct >( updateOnHandQuantityRequest, _syncRunContext, ctx );

			LightspeedLogger.Debug( _syncRunContext, CallerType, "Quantity updated successfully. " + paramInfo );
		}

		public async Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkusFull, CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Starting to get item sku index" );
			var itemSkusPartitioned = itemSkusFull.ToList().Partition( 100 );

			var dictionary = new Dictionary< string, LightspeedProduct >();

			var tasks = itemSkusPartitioned.Select( itemSkus =>
			{
				var getItemRequest = new GetItemsRequest( itemSkus );
				return this._webRequestServices.GetResponseAsync<LightspeedProductList>( getItemRequest, _syncRunContext, ctx );
			} );

			await Task.WhenAll( tasks );

			tasks.ForEach( t =>
			{
				if ( t.Result.Item != null )
				{
					LightspeedLogger.Debug( _syncRunContext, CallerType, $"Got {t.Result.Item.Length} entries in item sku index" );
					t.Result.Item.ToList().Distinct().ForEach( i =>
					{
						dictionary[ i.Sku ] = i;
					} );
				}
			} );
			return dictionary;
		}

		private async Task< IEnumerable< LightspeedProduct > > ExecuteGetItemsRequest( GetItemsRequest request, CancellationToken ctx ) {
			var result = new List< LightspeedProduct >();
			var response = await this._webRequestServices.GetResponseAsync<LightspeedProductList>( request, _syncRunContext, ctx );
			if ( response.Item != null )
				result = response.Item.ToList();
			return result;			
		} 

		public async Task< bool > DoesItemExistAsync( int itemId, CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, $"Checking, if item {itemId} exists" );
			var request = new GetItemRequest( itemId );
			var response = await this._webRequestServices.GetResponseAsync< LightspeedProduct >( request, _syncRunContext, ctx );
			return response != null;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItemsCreatedInShopAsync( int shopId, DateTime createTimeUtc, CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, string.Format( "Getting items, created in shop {0} after {1}", 
				shopId, createTimeUtc ) );

			var getItemRequest = new GetItemsRequest( shopId, createTimeUtc );
			var result = await this.ExecuteGetItemsRequest( getItemRequest, ctx );

			LightspeedLogger.Debug( _syncRunContext, CallerType, string.Format( "Getting {0} items updated after {1} in shop {2}", 
				result.Count(), createTimeUtc, shopId ) );
			return result;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx )
		{
			var getItemRequest = new GetItemsRequest( shopId );
			return await this.ExecuteGetItemsRequest( getItemRequest, ctx );
		}

		public async Task< IEnumerable< int > > GetExistingItemsIdsAsync( List< int > itemIds, CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, $"Checking, if items {itemIds.ToJson()} exists" );
			var existingProducts = await this.GetItemsAsync( itemIds.ToHashSet(), ctx );
			return existingProducts.Select( p => p.ItemId );
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( HashSet< int > itemIdsFull, CancellationToken ctx )
		{
			LightspeedLogger.Debug( _syncRunContext, CallerType, "Started getting products by IDs" );

			if( itemIdsFull.Count == 0 )
				return new List< LightspeedProduct >();

			var result = new List< LightspeedProduct >();
			var itemIdsPartitioned = itemIdsFull.ToList().Partition( GetItemsRequest.DefaultLimit );
			foreach( var itemIds in itemIdsPartitioned )
			{
				var getItemsRequest = new GetItemsRequest( itemIds );
				var response = await this._webRequestServices.GetResponseAsync< LightspeedProductList >( getItemsRequest, _syncRunContext, ctx );
				if( response != null && response.Item != null && response.Item.Length > 0 )
					result.AddRange( response.Item );
			}

			LightspeedLogger.Debug( _syncRunContext, CallerType, $"Got {result.Count} products by IDs" );
			return result;
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