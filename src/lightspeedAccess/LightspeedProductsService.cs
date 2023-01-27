using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Services;

namespace LightspeedAccess
{
	public class LightspeedProductsService: ILightspeedProductsService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly LightspeedConfig _config;
		private readonly int _accountId;

		public LightspeedProductsService( LightspeedConfig config, LightspeedAuthService authService )
		{
			LightspeedLogger.Debug( $"Started LightspeedProductsService with config {config}", this._accountId );
			this._webRequestServices = new WebRequestService( config, new ThrottlerAsync( config.AccountId ), authService );
			this._config = config;
			this._accountId = this._config.AccountId;
		}

		public async Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx )
		{
			var getProductsRequest = new GetProductsRequest( shopId );
			var products = await this.ExecuteGetProductsRequest( getProductsRequest, ctx );

			var getVendorsRequest = new GetVendorsRequest( shopId );
			var vendors = ( await this.ExecuteGetVendorsRequest( getVendorsRequest, ctx ) )
				.ToDictionary( k => k.VendorId, v => v.Name );

			foreach( var product in products )
			{
				if( !vendors.TryGetValue( product.DefaultVendorId, out var vendorName ) )
					continue;

				product.DefaultVendorName = vendorName;
			}

			return products;
		}

		private async Task< IEnumerable< LightspeedFullProduct > > ExecuteGetProductsRequest( GetProductsRequest request, CancellationToken ctx )
		{
			var result = new List< LightspeedFullProduct >();
			var response = await this._webRequestServices.GetResponseAsync< LightspeedFullProductList >( request, ctx );
			if( response.Item != null )
				result = response.Item.ToList();
			return result;
		}

		private async Task< IEnumerable< LightspeedVendor > > ExecuteGetVendorsRequest( GetVendorsRequest request, CancellationToken ctx )
		{
			var result = new List< LightspeedVendor >();
			var response = await this._webRequestServices.GetResponseAsync< LightspeedVendorList >( request, ctx );
			if( response.Vendor != null )
				result = response.Vendor.ToList();
			return result;
		}
	}
}