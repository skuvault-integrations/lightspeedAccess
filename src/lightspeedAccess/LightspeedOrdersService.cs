using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

		public LightspeedOrdersService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedOrdersService with config {0}", config.ToString() );
			this.throttler = new ThrottlerAsync();
			this._webRequestServices = new WebRequestService( config, this.throttler );
		}

		public IEnumerable< LightspeedOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			LightspeedLogger.Log.Debug( "Started getting orders from lightspeed from {0} to {1}", dateFrom, dateTo );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			ActionPolicies.Submit.Do( () =>
			{
				var response = this._webRequestServices.GetResponse< OrderList >( getSalesRequest );
				if( response.Sale != null )
					rawOrders = response.Sale.Where( s => s.SaleLines != null ).ToList();
			} );

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Log.Debug( "Got {0} raw orders", rawOrders.Count );

			var shopsNames = GetShopNames();
			var items = GetItems( rawOrders );
			var shipInfos = GetShipInfo( rawOrders );

			LightspeedLogger.Log.Debug( "Adding shop info, sale lines and ship info to raw orders..." );
			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if( shipInfos.ContainsKey( o.SaleId ) )
					o.ShipTo = shipInfos.GetValue( o.SaleId );

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			LightspeedLogger.Log.Debug( "Retrieving orders completed" );

			return rawOrders;
		}

		private IEnumerable< LightspeedProduct > GetItems( IEnumerable< LightspeedOrder > orders )
		{
			LightspeedLogger.Log.Debug( "Started getting products for orders" );

			var itemIds = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();

			var getItemsRequest = new GetItemsRequest( itemIds );

			var result = new List< LightspeedProduct >();

			ActionPolicies.Submit.Do(
				() =>
				{
					var response = this._webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest );
					if( response.Item != null )
						result = _webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest ).Item.ToList();
				} );

			LightspeedLogger.Log.Debug( "Got {0} products for orders", result.Count );

			return result;
		}

		private Dictionary< int, ShipTo > GetShipInfo( IEnumerable< LightspeedOrder > orders )
		{
			LightspeedLogger.Log.Debug( "Started getting shipping info for orders" );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = this._webRequestServices.GetResponse< ShipInfoList >( getShipInfoRequest ).ShipTo;
			if( response != null )
				result = response.Where( st => st.SaleId != 0 ).Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			LightspeedLogger.Log.Debug( "Got {0} shipping info entries", result.Count );
			return result;
		}

		private Dictionary< int, string > GetShopNames()
		{
			LightspeedLogger.Log.Debug( "Started getting shop names..." );

			var result = new Dictionary< int, string >();
			var response = this._webRequestServices.GetResponse< ShopsList >( new GetShopRequest() ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Log.Debug( "Got {0} shop entries", result.Count );

			return result;
		}

		private async Task< Dictionary< int, string > > GetShopNamesAsync( CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting shop names..." );
			var result = new Dictionary< int, string >();

			var response = ( await this._webRequestServices.GetResponseAsync< ShopsList >( new GetShopRequest(), ctx ) ).Shop;
			if( response != null )
				result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Log.Debug( "Got {0} shop entries", result.Count );

			return result;
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Log.Warn( "Started getting products for orders" );
			var itemIds = orders.Where( s => s.SaleLines != null ).ToList().SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ).Where( id => id != 0 ) ).ToHashSet();

			var result = new List< LightspeedProduct >();
			if( itemIds.Count == 0 )
				return result;

			var getItemsRequest = new GetItemsRequest( itemIds );

			var response = ( await this._webRequestServices.GetResponseAsync< LightspeedProductList >( getItemsRequest, ctx ) );
			if( response.Item != null )
				result = response.Item.ToList();

			LightspeedLogger.Log.Debug( "Got {0} products for orders", result.Count );
			return result;
		}

		private async Task< Dictionary< int, ShipTo > > GetShipInfoAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting shipping info for orders" );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			var response = ( await this._webRequestServices.GetResponseAsync< ShipInfoList >( getShipInfoRequest, ctx ) ).ShipTo;
			if( response != null )
				result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).Where( x => x.Item1 != 0 ).ToDictionary( x => x.Item1, x => x.Item2 );

			LightspeedLogger.Log.Debug( "Got {0} shipping info entries", result.Count );
			return result;
		}

		public async Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting orders from lightspeed from {0} to {1}", dateFrom, dateTo );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			var response =
				( await this._webRequestServices.GetResponseAsync< OrderList >( getSalesRequest, ctx ) );
			if( response.Sale != null )
				rawOrders = response.Sale.ToList();

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Log.Debug( "Got {0} raw orders", rawOrders.Count );

			var items = await this.GetItemsAsync( rawOrders, ctx );
			var shipInfos = await this.GetShipInfoAsync( rawOrders, ctx );
			var shopsNames = await this.GetShopNamesAsync( ctx );

			LightspeedLogger.Log.Debug( "Adding shop info, sale lines and ship info to raw orders..." );
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

				if( shipInfos.ContainsKey( o.SaleId ) )
					o.ShipTo = shipInfos.GetValue( o.SaleId );

				if( shopsNames.ContainsKey( o.ShopId ) )
					o.ShopName = shopsNames.GetValue( o.ShopId );
			} );

			LightspeedLogger.Log.Debug( "Retrieving orders completed" );
			return rawOrders;
		}
	}
}