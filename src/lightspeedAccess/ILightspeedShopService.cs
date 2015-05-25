using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess.Models.Shop;

namespace lightspeedAccess
{
	public interface ILightspeedShopService
	{
		IEnumerable< Shop > GetShops();
		Task< IEnumerable< Shop > > GetShopsAsync(CancellationToken ctx);
		void UpdateOnHandQuantity( int itemId, int shopId, int quantity );
		Task UpdateOnHandQuantityAsync( int itemId, int shopId, int quantity, CancellationToken ctx );
	}
}
