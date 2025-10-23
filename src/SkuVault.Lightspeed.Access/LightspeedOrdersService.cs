using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkuVault.Integrations.Core.Common;
using SkuVault.Integrations.Core.Logging;
using SkuVault.Lightspeed.Access.Extensions;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Models.Order;
using SkuVault.Lightspeed.Access.Models.Product;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Lightspeed.Access.Models.ShippingInfo;
using SkuVault.Lightspeed.Access.Models.Shop;

namespace SkuVault.Lightspeed.Access
{
	public class LightspeedOrdersService: LightspeedBaseService, ILightspeedOrdersService
	{
		private const string CallerType = nameof(LightspeedOrdersService);

		public LightspeedOrdersService( LightspeedConfig config, SyncRunContext syncRunContext, IIntegrationLogger logger ) :
			base(config, syncRunContext, logger)
		{
		}

		public IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Started getting orders from lightspeed from '{DateFrom} to {DateTo}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetOrders),
				dateFrom,
				dateTo );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = this._webRequestServices.GetResponse< OrderList >( getSalesRequest, this._syncRunContext );
			if( response.Sale != null )
				rawOrders = response.Sale.Where( s => s.SaleLines != null ).ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			var shopsNames = this.GetShopNames();
			var items = this.GetItems( rawOrders );

			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Retrieving orders completed. Got {RawOrdersCount} raw orders",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetOrders),
				rawOrders.Count );

			return rawOrders;
		}

		private IEnumerable< LightspeedProduct > GetItems( IEnumerable< LightspeedOrder > orders )
		{
			var itemIdsFull = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();
			return this.GetItems( itemIdsFull );
		}

		private IEnumerable< LightspeedProduct > GetItems( HashSet< int > itemIdsFull )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var itemIdsPartitioned = itemIdsFull.ToList().Partition( 100 );

			var result = new List< LightspeedProduct >();
			itemIdsPartitioned.ForEach( itemIds =>
			{
				var getItemsRequest = new GetItemsRequest( itemIds );
				getItemsRequest.SetArchivedOptionEnum( GetItemsRequest.ArchivedOptionEnum.True );

				var response = this._webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest, this._syncRunContext );
				if( response.Item != null )
					result.AddRange( this._webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest, this._syncRunContext ).Item.ToList() );
			} );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got {ProductsCount} products for orders",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetItems),
				result.Count );

			return result;
		}

		private Dictionary< int, string > GetShopNames()
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var result = new Dictionary< int, string >();
			var response = this._webRequestServices.GetResponse< ShopsList >( new GetShopRequest(), this._syncRunContext ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got {ShopCount} shop entries",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetShopNames),
				result.Count );

			return result;
		}

		private async Task< Dictionary< int, string > > GetShopNamesAsync( CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var result = new Dictionary< int, string >();

			var response = ( await this._webRequestServices.GetResponseAsync< ShopsList >( new GetShopRequest(), this._syncRunContext, ctx ) ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got {ShopCount} shop entries",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetShopNamesAsync),
				result.Count );

			return result;
		}

		private Task< IEnumerable< LightspeedProduct > > GetItemsAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			var itemIdsFull = orders
				.Where( s => s.SaleLines != null )
				.ToList()
				.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId )
				.Where( id => id != 0 ) )
				.ToHashSet();
			return this.GetItemsAsync( itemIdsFull, ctx );
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( HashSet< int > itemIdsFull, CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			if ( itemIdsFull.Count == 0 )
				return new List< LightspeedProduct >();

			var itemIdsPartitioned = itemIdsFull.ToList().Partition( 100 );
			var tasks = itemIdsPartitioned.Select( itemIds =>
			{
				var getItemsRequest = new GetItemsRequest( itemIds );
				getItemsRequest.SetArchivedOptionEnum( GetItemsRequest.ArchivedOptionEnum.True );

				return this._webRequestServices.GetResponseAsync< LightspeedProductList >( getItemsRequest, this._syncRunContext, ctx );
			} ).ToArray();
			await Task.WhenAll( tasks );
			var result = tasks.SelectMany( t => t.Result.Item ?? Array.Empty< LightspeedProduct >() ).ToList();

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got {ProductsCount} products for orders",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetItemsAsync),
				result.Count );

			return result;
		}

		private async Task< Dictionary< int, ShipTo > > GetShipInfoAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			_logger.LogOperationStart( _syncRunContext, CallerType );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = ( await this._webRequestServices.GetResponseAsync< ShipInfoList >( getShipInfoRequest, this._syncRunContext, ctx ) ).ShipTo;
			if( response != null )
				result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).Where( x => x.Item1 != 0 ).ToDictionary( x => x.Item1, x => x.Item2 );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Got {ShippingInfoCount} shipping info entries",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetShipInfoAsync),
				result.Count );

			return result;
		}

		public async Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx )
		{
			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Started getting orders from lightspeed from '{DateFrom} to {DateTo}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetOrdersAsync),
				dateFrom,
				dateTo );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = await this._webRequestServices.GetResponseAsync< OrderList >( getSalesRequest, this._syncRunContext, ctx );
			if( response.Sale != null )
				rawOrders = response.Sale.ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			var items = await this.GetItemsAsync( rawOrders, ctx );
			var shopsNames = await this.GetShopNamesAsync( ctx );

			rawOrders.ForEach( o =>
			{
				if( o.SaleLines != null && items.Count() != 0 )
				{
					o.SaleLines.ForEach( s =>
					{
						var item = items.ToList().Find( i => i.ItemId == s.ItemId );
						if( item != null )
							o.Products.Add( item );
					} );
				}

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Retrieving orders completed. Got {RawOrdersCount} raw orders",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetOrders),
				rawOrders.Count );

			return rawOrders;
		}
	}
}