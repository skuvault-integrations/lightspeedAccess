using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Shop;

namespace LightspeedAccess
{
	public interface ILightspeedShopService
	{
		IEnumerable< Shop > GetShops();
		Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx );
		void UpdateOnHandQuantity( int itemId, string sku, int shopId, string shopName, int itemShopRelationId, int quantity );
		Task UpdateOnHandQuantityAsync( int itemId, string sku, int shopId, string shopName, int itemShopRelationId, int quantity, CancellationToken ctx );
		Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkus, CancellationToken ctx );
		Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx );
		Task< IEnumerable< LightspeedProduct > > GetItemsCreatedInShopAsync( int shopId, DateTime modifiedUtc, CancellationToken ctx );
		Task< bool > DoesItemExist( int itemId, CancellationToken ctx );
	}
}