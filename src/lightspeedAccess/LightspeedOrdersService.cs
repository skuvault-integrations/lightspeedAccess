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

namespace LightspeedAccess
{
	public class LightspeedOrdersService: ILightspeedOrdersService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly ThrottlerAsync throttler;
		private readonly int _accountId;

		public LightspeedOrdersService( LightspeedConfig config, LightspeedAuthService authService )
		{
			this._accountId = config.AccountId;
			LightspeedLogger.Debug( string.Format( "Started LightspeedOrdersService with config {0}", config ), this._accountId );
			this.throttler = new ThrottlerAsync( config.AccountId );
			this._webRequestServices = new WebRequestService( config, this.throttler, authService );
			
		}

		public IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			LightspeedLogger.Debug( string.Format( "Started getting orders from lightspeed from {0} to {1}", dateFrom, dateTo ), this._accountId );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = this._webRequestServices.GetResponse< OrderList >( getSalesRequest );
			if( response.Sale != null )
				rawOrders = response.Sale.Where( s => s.SaleLines != null ).ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Debug( string.Format( "Got {0} raw orders", rawOrders.Count ), this._accountId );

			var shopsNames = this.GetShopNames();
			var items = this.GetItems( rawOrders );

			LightspeedLogger.Debug( "Adding shop info, sale lines and ship info to raw orders...", this._accountId );
			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			LightspeedLogger.Debug( "Retrieving orders completed", this._accountId );

			return rawOrders;
		}

		private IEnumerable< LightspeedProduct > GetItems( IEnumerable< LightspeedOrder > orders )
		{
			var itemIdsFull = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();
			return this.GetItems( itemIdsFull );
		}

		private IEnumerable< LightspeedProduct > GetItems( HashSet< int > itemIdsFull )
		{
			LightspeedLogger.Debug( "Started getting products for orders", this._accountId );

			var itemIdsPartitioned = itemIdsFull.ToList().Partition( 100 );

			var result = new List< LightspeedProduct >();
			itemIdsPartitioned.ForEach( itemIds =>
			{
				var getItemsRequest = new GetItemsRequest( itemIds );
				getItemsRequest.SetArchivedOptionEnum( GetItemsRequest.ArchivedOptionEnum.True );

				var response = this._webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest );
				if( response.Item != null )
					result.AddRange( this._webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest ).Item.ToList() );
			} );

			LightspeedLogger.Debug( string.Format( "Got {0} products for orders", result.Count ), this._accountId );

			return result;
		}

		private Dictionary< int, ShipTo > GetShipInfo( IEnumerable< LightspeedOrder > orders )
		{
			LightspeedLogger.Debug( "Started getting shipping info for orders", this._accountId );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = this._webRequestServices.GetResponse< ShipInfoList >( getShipInfoRequest ).ShipTo;
			if( response != null )
				result = response.Where( st => st.SaleId != 0 ).Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			LightspeedLogger.Debug( string.Format( "Got {0} shipping info entries", result.Count ), this._accountId );
			return result;
		}

		private Dictionary< int, string > GetShopNames()
		{
			LightspeedLogger.Debug( "Started getting shop names...", this._accountId );

			var result = new Dictionary< int, string >();
			var response = this._webRequestServices.GetResponse< ShopsList >( new GetShopRequest() ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( string.Format( "Got {0} shop entries", result.Count ), this._accountId );

			return result;
		}

		private async Task< Dictionary< int, string > > GetShopNamesAsync( CancellationToken ctx )
		{
			LightspeedLogger.Debug( "Started getting shop names...", this._accountId );
			var result = new Dictionary< int, string >();

			var response = ( await this._webRequestServices.GetResponseAsync< ShopsList >( new GetShopRequest(), ctx ) ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( string.Format( "Got {0} shop entries", result.Count ), this._accountId );

			return result;
		}

		private Task< IEnumerable< LightspeedProduct > > GetItemsAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			var itemIdsFull = orders.Where( s => s.SaleLines != null ).ToList().SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ).Where( id => id != 0 ) ).ToHashSet();
			return this.GetItemsAsync( itemIdsFull, ctx );
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( HashSet< int > itemIdsFull, CancellationToken ctx )
		{
			LightspeedLogger.Debug( "Started getting products for orders", this._accountId );

			if( itemIdsFull.Count == 0 )
				return new List< LightspeedProduct >();

			var itemIdsPartitioned = itemIdsFull.ToList().Partition( 100 );
			var tasks = itemIdsPartitioned.Select( itemIds =>
			{
				var getItemsRequest = new GetItemsRequest( itemIds );
				getItemsRequest.SetArchivedOptionEnum( GetItemsRequest.ArchivedOptionEnum.True );

				return this._webRequestServices.GetResponseAsync< LightspeedProductList >( getItemsRequest, ctx );
			} ).ToArray();
			await Task.WhenAll( tasks );
			var result = tasks.SelectMany( t => t.Result.Item ?? new LightspeedProduct[ 0 ] ).ToList();

			LightspeedLogger.Debug( string.Format( "Got {0} products for orders", result.Count ), this._accountId );
			return result;
		}

		private async Task< Dictionary< int, ShipTo > > GetShipInfoAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Debug( "Started getting shipping info for orders", this._accountId );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = ( await this._webRequestServices.GetResponseAsync< ShipInfoList >( getShipInfoRequest, ctx ) ).ShipTo;
			if( response != null )
				result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).Where( x => x.Item1 != 0 ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Debug( string.Format( "Got {0} shipping info entries", result.Count ), this._accountId );
			return result;
		}

		public async Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx )
		{
			LightspeedLogger.Debug( string.Format( "Started getting orders from lightspeed from {0} to {1}", dateFrom, dateTo ), this._accountId );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response = await this._webRequestServices.GetResponseAsync< OrderList >( getSalesRequest, ctx );
			if( response.Sale != null )
				rawOrders = response.Sale.ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Debug( string.Format( "Got {0} raw orders", rawOrders.Count ), this._accountId );

			var items = await this.GetItemsAsync( rawOrders, ctx );
			var shopsNames = await this.GetShopNamesAsync( ctx );

			LightspeedLogger.Debug( "Adding shop info, sale lines and ship info to raw orders...", this._accountId );
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

			LightspeedLogger.Debug( "Retrieving orders completed", this._accountId );
			return rawOrders;
		}
	}
}