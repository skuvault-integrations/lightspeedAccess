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

		public async Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, DateTime createTimeUtc, CancellationToken ctx )
		{
			LightspeedLogger.Debug( $"Getting products, created in shop {shopId} after {createTimeUtc}", this._accountId );
			var getProductsRequest = new GetProductsRequest( shopId, createTimeUtc );
			var result = await this.ExecuteGetProductsRequest( getProductsRequest, ctx );
			LightspeedLogger.Debug( $"Getting {result.Count()} products updated after {createTimeUtc} in shop {shopId}", this._accountId );
			return result;
		}

		public async Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx )
		{
			var getProductsRequest = new GetProductsRequest( shopId );
			return await this.ExecuteGetProductsRequest( getProductsRequest, ctx );
		}

		private async Task< IEnumerable< LightspeedFullProduct > > ExecuteGetProductsRequest( GetProductsRequest request, CancellationToken ctx )
		{
			var result = new List< LightspeedFullProduct >();
			var response = await this._webRequestServices.GetResponseAsync< LightspeedFullProductList >( request, ctx );
			if( response.Item != null )
				result = response.Item.ToList();
			return result;
		}
	}
}