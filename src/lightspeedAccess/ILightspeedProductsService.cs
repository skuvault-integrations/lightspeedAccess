using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Models.Product;

namespace LightspeedAccess
{
	public interface ILightspeedProductsService
	{
		Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, DateTime createTimeUtc, CancellationToken ctx );
		
		Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx );
	}
}