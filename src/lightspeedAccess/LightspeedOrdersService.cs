using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Order;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Models.ShippingInfo;
using lightspeedAccess.Services;
using Netco.Extensions;

namespace lightspeedAccess
{
	internal class LightspeedOrdersService: ILightspeedOrdersService
	{
		private readonly WebRequestService _webRequestServices;

		public LightspeedOrdersService( LightspeedConfig config )
		{
			_webRequestServices = new WebRequestService( config );
		}

		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List< Order >();

			ActionPolicies.Submit.Do( () =>
			{
				rawOrders =
					_webRequestServices.GetResponse< OrderList >( getSalesRequest ).Sale.Where( s => s.SaleLines != null ).ToList();
			} );

			var items = GetItems( rawOrders );
			var shipInfos = GetShipInfo( rawOrders );

			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if( shipInfos.ContainsKey( o.SaleId ) )
					o.ShipTo = shipInfos.GetValue( o.SaleId );
			} );

			return rawOrders;
		}

		private IEnumerable< LightspeedProduct > GetItems( IEnumerable< Order > orders )
		{
			var itemIds = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();
			var getItemsRequest = new GetItemsRequest( itemIds );

			var result = new List< LightspeedProduct >();

			ActionPolicies.Submit.Do(
				() =>
				{
					result = _webRequestServices.GetResponse< LightspeedProductList >( getItemsRequest ).Item.ToList();
				} );

			return result;
		}

		private Dictionary< int, ShipTo > GetShipInfo( IEnumerable< Order > orders )
		{
			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary< int, ShipTo >();
			ActionPolicies.Submit.Do( () =>
			{
				var response = _webRequestServices.GetResponse< ShipInfoList >( getShipInfoRequest ).ShipTo;
				if( response != null )
					result = response.Select( st => new Tuple< int, ShipTo >( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			} );

			return result;
		}

		private async Task<IEnumerable<LightspeedProduct>> GetItemsAsync( IEnumerable<Order> orders, CancellationToken ctx )
		{
			var itemIds = orders.SelectMany( o => o.SaleLines.Select( sl => sl.ItemId ) ).ToHashSet();
			var getItemsRequest = new GetItemsRequest( itemIds );

			var result = new List<LightspeedProduct>();

			await ActionPolicies.SubmitAsync.Do(
				async () =>
				{
					result = (await _webRequestServices.GetResponseAsync<LightspeedProductList>( getItemsRequest, ctx )).Item.ToList();
				} );

			return result;
		}

		private async Task<Dictionary<int, ShipTo>> GetShipInfoAsync( IEnumerable<Order> orders, CancellationToken ctx )
		{
			var shipIds = orders.Select( o => o.ShipToId ).ToHashSet();
			var getShipInfoRequest = new GetShipInfoRequest( shipIds );

			var result = new Dictionary<int, ShipTo>();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var response = (await _webRequestServices.GetResponseAsync<ShipInfoList>( getShipInfoRequest, ctx )).ShipTo;
				if ( response != null )
					result = response.Select( st => new Tuple<int, ShipTo>( st.SaleId, st ) ).ToDictionary( x => x.Item1, x => x.Item2 );
			} );

			return result;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, CancellationToken ctx)
		{
			var getSalesRequest = new GetSalesRequest( dateFrom, dateTo );

			var rawOrders = new List<Order>();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				rawOrders =
					(await _webRequestServices.GetResponseAsync<OrderList>( getSalesRequest, ctx )).Sale.Where( s => s.SaleLines != null ).ToList();
			} );

			var items = await GetItemsAsync( rawOrders, ctx );
			var shipInfos = await GetShipInfoAsync( rawOrders, ctx );

			rawOrders.ForEach( o =>
			{
				o.SaleLines.ForEach( s =>
					o.Products.Add( items.ToList().Find( i => i.ItemId == s.ItemId ) )
					);

				if ( shipInfos.ContainsKey( o.SaleId ) )
					o.ShipTo = shipInfos.GetValue( o.SaleId );
			} );

			return rawOrders;
		}
	}
}