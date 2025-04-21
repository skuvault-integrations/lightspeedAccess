using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Order;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Models.ShippingInfo;
using LightspeedAccess.Models.Shop;
using LightspeedAccess.Services;
using Netco.Extensions;
using SkuVault.Integrations.Core.Common;

namespace LightspeedAccess
{
	public class LightspeedOrdersService: ILightspeedOrdersService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly SyncRunContext _syncRunContext;
		private const string CallerType = nameof(LightspeedOrdersService);

		public LightspeedOrdersService( LightspeedConfig config, LightspeedAuthService authService, SyncRunContext syncRunContext )
		{
			this._syncRunContext = syncRunContext;
			var throttler = new ThrottlerAsync( config.AccountId, syncRunContext );
			this._webRequestServices = new WebRequestService( config, throttler, authService );
			
		}

		public IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType,
				$"Started getting orders from lightspeed from {dateFrom} to {dateTo}" );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = this._webRequestServices.GetResponse< OrderList >( getSalesRequest, this._syncRunContext );
			if( response.Sale != null )
				rawOrders = response.Sale.Where( s => s.SaleLines != null ).ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {rawOrders.Count} raw orders" );

			var shopsNames = this.GetShopNames();
			var items = this.GetItems( rawOrders );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Adding shop info, sale lines and ship info to raw orders..." );
			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Retrieving orders completed" );

			return rawOrders;
		}

		private IEnumerable< LightspeedProduct > GetItems( IEnumerable< LightspeedOrder > orders )
		{
			var itemIdsFull = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();
			return this.GetItems( itemIdsFull );
		}

		private IEnumerable< LightspeedProduct > GetItems( HashSet< int > itemIdsFull )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting products for orders" );

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

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} products for orders" );

			return result;
		}

		private Dictionary< int, ShipTo > GetShipInfo( IEnumerable< LightspeedOrder > orders )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting shipping info for orders" );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = this._webRequestServices.GetResponse< ShipInfoList >( getShipInfoRequest, this._syncRunContext ).ShipTo;
			if( response != null )
				result = response.Where( st => st.SaleId != 0 ).Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} shipping info entries" );
			return result;
		}

		private Dictionary< int, string > GetShopNames()
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting shop names..." );

			var result = new Dictionary< int, string >();
			var response = this._webRequestServices.GetResponse< ShopsList >( new GetShopRequest(), this._syncRunContext ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} shop entries" );

			return result;
		}

		private async Task< Dictionary< int, string > > GetShopNamesAsync( CancellationToken ctx )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting shop names..." );
			var result = new Dictionary< int, string >();

			var response = ( await this._webRequestServices.GetResponseAsync< ShopsList >( new GetShopRequest(), this._syncRunContext, ctx ) ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} shop entries" );

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
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting products for orders" );

			if( itemIdsFull.Count == 0 )
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

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} products for orders" );
			return result;
		}

		private async Task< Dictionary< int, ShipTo > > GetShipInfoAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Started getting shipping info for orders" );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = ( await this._webRequestServices.GetResponseAsync< ShipInfoList >( getShipInfoRequest, this._syncRunContext, ctx ) ).ShipTo;
			if( response != null )
				result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).Where( x => x.Item1 != 0 ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {result.Count} shipping info entries" );
			return result;
		}

		public async Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx )
		{
			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Started getting orders from lightspeed from {dateFrom} to {dateTo}" );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = await this._webRequestServices.GetResponseAsync< OrderList >( getSalesRequest, this._syncRunContext, ctx );
			if( response.Sale != null )
				rawOrders = response.Sale.ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Debug( this._syncRunContext, CallerType, $"Got {rawOrders.Count} raw orders" );

			var items = await this.GetItemsAsync( rawOrders, ctx );
			var shopsNames = await this.GetShopNamesAsync( ctx );

			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Adding shop info, sale lines and ship info to raw orders..." );
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

			LightspeedLogger.Debug( this._syncRunContext, CallerType, "Retrieving orders completed" );
			return rawOrders;
		}
	}
}