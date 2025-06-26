using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Models.Product;

namespace lightspeedAccess
{
	public interface ILightspeedProductsService
	{
		Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx );
	}
}