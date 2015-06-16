using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Models.Shop;

namespace LightspeedAccess
{
	public interface ILightspeedShopService
	{
		IEnumerable< Shop > GetShops();
		Task< IEnumerable< Shop > > GetShopsAsync(CancellationToken ctx);
		void UpdateOnHandQuantity( int itemId, int shopId, int quantity );
		Task UpdateOnHandQuantityAsync( int itemId, int shopId, int quantity, CancellationToken ctx );
		Task<IDictionary<string, int>> GetItems( IEnumerable<string> itemSkus, CancellationToken ctx );
	}
}
