using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkuVault.Lightspeed.Access.Models.Product;

namespace SkuVault.Lightspeed.Access
{
	public interface ILightspeedProductsService
	{
		Task< IEnumerable< LightspeedFullProduct > > GetProductsAsync( int shopId, CancellationToken ctx );
	}
}