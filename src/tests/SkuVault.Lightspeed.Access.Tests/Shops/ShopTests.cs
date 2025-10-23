using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace SkuVault.Lightspeed.Access.Tests.Shops
{
	internal class ShopTests : BaseTests
	{
		private readonly Randomizer _randomizer = new Randomizer();

		[ Explicit ]
		[ Test ]
		public async Task GetShopsAsync()
		{
			var service = GetShopsService();

			var shops = await service.GetShopsAsync( new CancellationToken() );

			Assert.Greater( shops.Count(), 0 );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateOnHandQuantityAsync()
		{
			var service = GetShopsService();
			var itemId = 17;
			var shopId = 1;
			var itemShopRelationId = 37;
			var quantity = _randomizer.Next( 1, 100 );

			await service.UpdateOnHandQuantityAsync( itemId, shopId, itemShopRelationId, quantity, new CancellationToken() );

			var items = await service.GetItems( shopId, new CancellationToken() );
			var item = items.FirstOrDefault( f => f.ItemId == itemId );
			var itemQty = item.ItemShops.FirstOrDefault( f => f.ShopId == shopId && f.ItemShopId == itemShopRelationId )?.QuantityOnHand;
			Assert.That( itemQty, Is.EqualTo( quantity ) );
		}

		[ Explicit ]
		[ Test ]
		public async Task GetItemsAsync_ReturnsItems_WhenCorrectSkuIsProvided()
		{
			var service = GetShopsService();
			var sku = "testsku1";

			var items = await service.GetItems( new List< string > { sku }, new CancellationToken() );

			Assert.Greater( items.Count, 0 );
		}

		[ Explicit ]
		[ Test ]
		public async Task GetItems_ReturnsItems_WhenCorrectShopIdIsProvided()
		{
			var service = GetShopsService();
			var shopId = 1;

			var items = await service.GetItems( shopId, new CancellationToken() );

			Assert.Greater( items.Count(), 0 );
		}

		[ Explicit ]
		[ Test ]
		public async Task GetExistingItemsIdsAsync_ReturnsSortedExistingItemsOnly_WhenCorrectAndFakeIdsProvided()
		{
			var service = GetShopsService();

			var items = await service.GetItems( 1, new CancellationToken() );
			var correctIds = items.Select( p => p.ItemId ).ToList();
			var fakeIds = new List< int > { -1, 99999990, 99999991, 99999992, 99999993, 99999994, 99999995 };
			var allIds = correctIds.Union( fakeIds ).ToList();
			var existingItems = await service.GetExistingItemsIdsAsync( allIds, new CancellationToken() );
			var resultIds = existingItems.ToList();

			Assert.AreEqual( resultIds.Count, correctIds.Count );
			correctIds.Sort();
			resultIds.Sort();
			for (var i = 0; i < resultIds.Count; i++)
			{
				Assert.AreEqual( resultIds[ i ], correctIds[ i ] );
			}
		}

		private ILightspeedShopService GetShopsService()
		{
			var provider = CreatePublicServiceProvider();
			var factory = provider.GetRequiredService<ILightspeedFactory>();
			return factory.CreateShopsService(_config, SyncRunContext);
		}
	}
}