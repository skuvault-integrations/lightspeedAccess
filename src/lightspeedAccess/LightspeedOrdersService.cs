using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
	internal class LightspeedOrdersService: ILightspeedOrdersService
	{
		private readonly WebRequestService _webRequestServices;

		public LightspeedOrdersService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedOrdersService with config {0}", config.ToString() );
			_webRequestServices = new WebRequestService( config );
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
			ActionPolicies.Submit.Do( () =>
			{
				var response = _webRequestServices.GetResponse< ShipInfoList >( getShipInfoRequest ).ShipTo;
				if( response != null )
					result = response.Where( st => st.SaleId != 0 ).Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			} );

			LightspeedLogger.Log.Debug( "Got {0} shipping info entries", result.Count );
			return result;
		}

		private Dictionary< int, string > GetShopNames()
		{
			LightspeedLogger.Log.Debug( "Started getting shop names..." );

			var result = new Dictionary< int, string >();
			ActionPolicies.Submit.Do( () =>
			{
				var response = _webRequestServices.GetResponse< ShopsList >( new GetShopRequest() ).Shop;
				if( response != null )
					result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			} );

			LightspeedLogger.Log.Debug( "Got {0} shop entries", result.Count );

			return result;
		}

		private async Task< Dictionary< int, string > > GetShopNamesAsync( CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting shop names..." );
			var result = new Dictionary< int, string >();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var response = ( await _webRequestServices.GetResponseAsync< ShopsList >( new GetShopRequest(), ctx ) ).Shop;
				if( response != null )
					result = response.Select( st => new Tuple< int, string >( st.ShopId, st.ShopName ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			} );

			LightspeedLogger.Log.Debug( "Got {0} shop entries", result.Count );

			return result;
		}

		private async Task< IEnumerable< LightspeedProduct > > GetItemsAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting products for orders" );
			var itemIds = orders.Where( s => s.SaleLines != null ).ToList().SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ).Where( id => id != 0 ) ).ToHashSet();

			var result = new List< LightspeedProduct >();
			if( itemIds.Count == 0 )
				return result;

			var getItemsRequest = new GetItemsRequest( itemIds );

			await ActionPolicies.SubmitAsync.Do(
				async () =>
				{
					var response = ( await _webRequestServices.GetResponseAsync< LightspeedProductList >( getItemsRequest, ctx ) );
					if( response.Item != null )
						result = response.Item.ToList();
				} );

			LightspeedLogger.Log.Debug( "Got {0} products for orders", result.Count );
			return result;
		}

		private async Task< Dictionary< int, ShipTo > > GetShipInfoAsync( IEnumerable< LightspeedOrder > orders, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting shipping info for orders" );

			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				try
				{
					var response = ( await _webRequestServices.GetResponseAsync< ShipInfoList >( getShipInfoRequest, ctx ) ).ShipTo;
					if( response != null )
						result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).Where( x => x.Item1 != 0 ).ToDictionary( x => x.Item1, x => x.Item2 );
				}
				catch( WebException e )
				{
					var reader2 = new StreamReader( e.Response.GetResponseStream() );
					LightspeedLogger.Log.Debug( "Could not retrieve order shipping info ({0} : {1}). Probably because of older channel account, that has insufficient permissions for that. Please, create another channel account with the same credetials and disable this one", reader2.ReadToEnd(), e.Message );
				}
			} );

			LightspeedLogger.Log.Debug( "Got {0} shipping info entries", result.Count );
			return result;
		}

		public async Task< IEnumerable< LightspeedOrder > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Started getting orders from lightspeed from {0} to {1}", dateFrom, dateTo );

			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< LightspeedOrder >();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var response =
					( await _webRequestServices.GetResponseAsync< OrderList >( getSalesRequest, ctx ) );
				if( response.Sale != null )
					rawOrders = response.Sale.ToList();
			} );

			if( rawOrders.Count == 0 )
				return rawOrders;

			LightspeedLogger.Log.Debug( "Got {0} raw orders", rawOrders.Count );

			var items = await GetItemsAsync( rawOrders, ctx );
			var shipInfos = await GetShipInfoAsync( rawOrders, ctx );
			var shopsNames = await GetShopNamesAsync( ctx );

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