using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
	internal class LightspeedShopService: ILightspeedShopService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly LightspeedConfig _config;

		public LightspeedShopService( LightspeedConfig config )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedShopsService with config {0}", config.ToString() );
			_webRequestServices = new WebRequestService( config );
			_config = config;
		}

		public IEnumerable< Shop > GetShops()
		{
			LightspeedLogger.Log.Debug( "Starting to get Shops" );
			var getShopsRequest = new GetShopRequest();
			var shops = _webRequestServices.GetResponse< ShopsList >( getShopsRequest ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public async Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting to get Shops" );

			var getShopsRequest = new GetShopRequest();
			var shops = ( await _webRequestServices.GetResponseAsync< ShopsList >( getShopsRequest, ctx ) ).Shop;
			if( shops == null )
				return new List< Shop >();

			LightspeedLogger.Log.Debug( "Retrieved {0} shops", shops.Length );
			return shops.ToList();
		}

		public void UpdateOnHandQuantity( int itemId, int shopId, int itemShopRelationID, int quantity )
		{
			LightspeedLogger.Log.Debug( "Starting update shop item quantity" );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationID, quantity );
			_webRequestServices.GetResponse< LightspeedProduct >( updateOnHandQuantityRequest );
			LightspeedLogger.Log.Debug( "Quantity updated successfully" );
		}

		public async Task UpdateOnHandQuantityAsync( int itemId, int shopId, int itemShopRelationId, int quantity, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting update shop item quantity" );
			var updateOnHandQuantityRequest = new UpdateOnHandQuantityRequest( itemId, shopId, itemShopRelationId, quantity );
			await ActionPolicies.SubmitAsync.Do( async () =>
				await _webRequestServices.GetResponseAsync< LightspeedProduct >( updateOnHandQuantityRequest, ctx )
				);

			LightspeedLogger.Log.Debug( "Quantity updated successfully" );
		}

		public async Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkus, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Starting to get item sku index" );
			var getItemRequest = new GetItemsRequest( itemSkus );

			var dictionary = new Dictionary< string, LightspeedProduct >();

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var result = await _webRequestServices.GetResponseAsync< LightspeedProductList >( getItemRequest, ctx );
				if( result.Item != null )
				{
					LightspeedLogger.Log.Debug( "Got {0} entries in item sku index", result.Item.Length );
					result.Item.ToList().Distinct().ForEach( i =>
					{
						dictionary[ i.Sku ] = i;
					} );
				}
			} );
			return dictionary;
		}

		public async Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx )
		{
			var getItemRequest = new GetItemsRequest( shopId );
			var result = new List< LightspeedProduct >();
			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var response = await _webRequestServices.GetResponseAsync< LightspeedProductList >( getItemRequest, ctx );
				if( response.Item != null )
					result = response.Item.ToList();
			} );

			return result;
		}

		public ShopOrder MakeOrderRequest< T >( string endpoint, string token, T body, string method ) where T : ShopOrderBase
		{
			var uri = new Uri( _config.Endpoint + endpoint + "?oauth_token=" + token );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = method;
			var serializer = new XmlSerializer( typeof( T ) );
			Stream requestStream = new System.IO.MemoryStream();

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