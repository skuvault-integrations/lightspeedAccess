using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkuVault.Integrations.Core.Common;
using SkuVault.Integrations.Core.Logging;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Models.Product;
using SkuVault.Lightspeed.Access.Models.Request;

namespace SkuVault.Lightspeed.Access
{
	public class LightspeedProductsService: LightspeedBaseService, ILightspeedProductsService
	{
		public LightspeedProductsService(LightspeedConfig config, SyncRunContext syncRunContext, IIntegrationLogger logger) :
			base(config, syncRunContext, logger)
		{
		}

		public async Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx )
		{
			var getProductsRequest = new GetProductsRequest( shopId );
			var products = await this.ExecuteGetProductsRequest( getProductsRequest, ctx );

			var getVendorsRequest = new GetVendorsRequest( shopId );
			var vendors = ( await this.ExecuteGetVendorsRequest( getVendorsRequest, ctx ) )
				.GroupBy( x => x.VendorId )
				.ToDictionary( k => k.Key, v => v.First().Name );

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
			var response = await this._webRequestServices.GetResponseAsync< LightspeedFullProductList >( request, _syncRunContext, ctx );
			if( response.Item != null )
				result = response.Item.ToList();
			return result;
		}

		private async Task< IEnumerable< LightspeedVendor > > ExecuteGetVendorsRequest( GetVendorsRequest request, CancellationToken ctx )
		{
			var result = new List< LightspeedVendor >();
			var response = await this._webRequestServices.GetResponseAsync< LightspeedVendorList >( request, _syncRunContext, ctx );
			if( response.Vendor != null )
				result = response.Vendor.ToList();
			return result;
		}
	}
}