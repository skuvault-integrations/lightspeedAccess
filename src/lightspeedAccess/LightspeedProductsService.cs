using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Services;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccess
{
	public class LightspeedProductsService: ILightspeedProductsService
	{
		private readonly WebRequestService _webRequestServices;
		private readonly SyncRunContext _syncRunContext;

		public LightspeedProductsService( LightspeedConfig config, LightspeedAuthService authService, SyncRunContext syncRunContext )
		{
			this._webRequestServices = new WebRequestService( config, new ThrottlerAsync( config.AccountId, syncRunContext ), authService );
			this._syncRunContext = syncRunContext;
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