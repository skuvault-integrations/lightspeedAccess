using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Misc;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Lightspeed.Access.Models.Shop;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Extensions;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Models.Product;
using SkuVault.Lightspeed.Access.Services;
using SkuVault.Integrations.Core.Logging;
using Microsoft.Extensions.Logging;

namespace SkuVault.Lightspeed.Access
{
	public class LightspeedShopService: LightspeedBaseService, ILightspeedShopService
	{
		private readonly WebRequestService _webRequestServicesForUpdating;
		private const string CallerType = nameof(LightspeedShopService);

		public LightspeedShopService(LightspeedConfig config, SyncRunContext syncRunContext, IIntegrationLogger logger) :
			base(config, syncRunContext, logger)
		{
			_webRequestServicesForUpdating = new WebRequestService(config,
					new ThrottlerAsync(ThrottlerConfig.CreateDefaultForWriteRequests(config.AccountId), syncRunContext, logger), _authService, logger);
		}

		public IEnumerable< Shop > GetShops()
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var getShopsRequest = new GetShopRequest();
			var shops = this._webRequestServices.GetResponse< ShopsList >( getShopsRequest, _syncRunContext ).Shop;
			if( shops == null )
				return new List< Shop >();

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Retrieved '{ShopsCount}' shops",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetShops),
				shops.Length );

			return shops.ToList();
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var getShopsRequest = new GetShopRequest();
			var shops = ( await this._webRequestServices.GetResponseAsync< ShopsList >( getShopsRequest, _syncRunContext, ctx ) ).Shop;
			if( shops == null )
				return new List< Shop >();

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Retrieved '{ShopsCount}' shops",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetShopsAsync),
				shops.Length );

			return shops.ToList();
		}

		public void UpdateOnHandQuantity( int itemId, int shopId, int itemShopRelationId, int quantity, string logComment = null )
		{
			var paramInfo = string.Format( "itemId:{0}, shopId:{1}, itemShopRelationId:{2}, quantity:{3}{4}",
				itemId, shopId, itemShopRelationId, quantity, ( !string.IsNullOrWhiteSpace( logComment ) ? ", " : "" ) + logComment );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Starting update shop item quantity: '{ParamInfo}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(UpdateOnHandQuantity),
				paramInfo );

			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			this._webRequestServicesForUpdating.GetResponse< LightspeedProduct >( updateOnHandQuantityRequest, _syncRunContext );

			_logger.LogOperationEnd( _syncRunContext, CallerType );
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, int shopId, int itemShopRelationId, int quantity, 
			CancellationToken ctx, string logComment = null )
		{
			var paramInfo = string.Format( "itemId:{0}, shopId:{1}, itemShopRelationId:{2}, quantity:{3}{4}",
				itemId, shopId, itemShopRelationId, quantity, ( !string.IsNullOrWhiteSpace( logComment ) ? ", " : "" ) + logComment );
			
			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Starting update shop item quantity: '{ParamInfo}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(UpdateOnHandQuantityAsync),
				paramInfo );

			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			await this._webRequestServicesForUpdating.GetResponseAsync< LightspeedProduct >( updateOnHandQuantityRequest, _syncRunContext, ctx );

			_logger.LogOperationEnd( _syncRunContext, CallerType );
		}

		public async Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkusFull, CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

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
					_logger.Logger.LogInformation(
						Constants.LoggingCommonPrefix + "Got '{ItemLength}' entries in item sku index",
						Constants.ChannelName,
						Constants.VersionInfo,
						_syncRunContext.TenantId,
						_syncRunContext.ChannelAccountId,
						_syncRunContext.CorrelationId,
						CallerType,
						nameof(GetItems),
						t.Result.Item.Length );

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
			var request = new GetItemRequest( itemId );
			var response = await this._webRequestServices.GetResponseAsync< LightspeedProduct >( request, _syncRunContext, ctx );
			return response != null;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItemsCreatedInShopAsync( int shopId, DateTime createTimeUtc, CancellationToken ctx )
		{
			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Getting items, created in shop '{ShopId}' after '{CreateTimeUtc}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetItemsCreatedInShopAsync),
				shopId,
				createTimeUtc );

			var getItemRequest = new GetItemsRequest( shopId, createTimeUtc );
			var result = await this.ExecuteGetItemsRequest( getItemRequest, ctx );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Getting '{ItemsNumber}' items updated after '{CreateTimeUtc}' in shop '{ShopId}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetItemsCreatedInShopAsync),
				result.Count(),
				createTimeUtc,
				shopId );

			return result;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx )
		{
			var getItemRequest = new GetItemsRequest( shopId );
			return await this.ExecuteGetItemsRequest( getItemRequest, ctx );
		}

		public async Task< IEnumerable< int > > GetExistingItemsIdsAsync( List< int > itemIds, CancellationToken ctx )
		{
			var existingProducts = await this.GetItemsAsync( itemIds.ToHashSet(), ctx );
			return existingProducts.Select( p => p.ItemId );
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( HashSet< int > itemIdsFull, CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

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

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got '{ProductsCount}' products by IDs",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetItemsCreatedInShopAsync),
				result.Count() );

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