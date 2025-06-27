using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkuVault.Lightspeed.Access.Models.Product;
using SkuVault.Lightspeed.Access.Models.Shop;

namespace SkuVault.Lightspeed.Access
{
	public interface ILightspeedShopService
	{
		IEnumerable< Shop > GetShops();
		Task< IEnumerable< Shop > > GetShopsAsync( CancellationToken ctx );
		void UpdateOnHandQuantity( int itemId, int shopId, int itemShopRelationId, int quantity, string logComment = null);
		Task UpdateOnHandQuantityAsync( int itemId, int shopId, int itemShopRelationId, int quantity, CancellationToken ctx, string logComment = null );
		Task< IDictionary< string, LightspeedProduct > > GetItems( IEnumerable< string > itemSkus, CancellationToken ctx );
		Task< IEnumerable< LightspeedProduct > > GetItems( int shopId, CancellationToken ctx );
		Task< IEnumerable< LightspeedProduct > > GetItemsCreatedInShopAsync( int shopId, DateTime modifiedUtc, CancellationToken ctx );
		Task< bool > DoesItemExistAsync( int itemId, CancellationToken ctx );
		Task< IEnumerable< int > > GetExistingItemsIdsAsync( List< int > itemIds, CancellationToken ctx );
	}
}