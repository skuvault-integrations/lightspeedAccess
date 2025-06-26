using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess;
using lightspeedAccess.Models.Configuration;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccessTests.Shops
{
	internal class ShopTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;
		private ILightspeedShopService _service;
		private readonly Randomizer _randomizer = new Randomizer();
		private static SyncRunContext SyncRunContext => new SyncRunContext( 1, 2, Guid.NewGuid().ToString() );

		[ SetUp ]
		public void Init()
		{
			var credentials = new Credentials.TestsCredentials(@"..\..\Files\lightspeedCredentials.csv");
			this._factory = new LightspeedFactory(credentials.ClientId, credentials.ClientSecret, "");
			this._config = new LightspeedConfig(credentials.AccountId, credentials.AccessToken, credentials.RefreshToken);
			this._service = this._factory.CreateShopsService( _config, SyncRunContext );
		}

		[ Test ]
		public async Task GetShopsAsync()
		{
			var shops = await this._service.GetShopsAsync( new CancellationToken() );

			Assert.Greater( shops.Count(), 0 );
		}

		[ Test ]
		public async Task UpdateOnHandQuantityAsync()
		{
			var itemId = 17;
			var shopId = 1;
			var itemShopRelationId = 37;
			var quantity = _randomizer.Next( 1, 100 );

			await this._service.UpdateOnHandQuantityAsync( itemId, shopId, itemShopRelationId, quantity, new CancellationToken() );

			var items = await this._service.GetItems( shopId, new CancellationToken() );
			var item = items.FirstOrDefault( f => f.ItemId == itemId );
			var itemQty = item.ItemShops.FirstOrDefault( f => f.ShopId == shopId && f.ItemShopId == itemShopRelationId )?.QuantityOnHand;
			Assert.That( itemQty, Is.EqualTo( quantity ) );
		}

		[ Test ]
		public async Task GetItemsAsync_ReturnsItems_WhenCorrectSkuIsProvided()
		{
			var sku = "testsku1";

			var items = await this._service.GetItems( new List< string > { sku }, new CancellationToken() );
			
			Assert.Greater( items.Count, 0 );
		}

		[ Test ]
		public async Task GetItems_ReturnsItems_WhenCorrectShopIdIsProvided()
		{
			var shopId = 1;

			var items = await this._service.GetItems( shopId, new CancellationToken() );
			
			Assert.Greater( items.Count(), 0 );
		}

		[ Test ]
		public async Task GetExistingItemsIdsAsync_ReturnsSortedExistingItemsOnly_WhenCorrectAndFakeIdsProvided()
		{
			var items = await this._service.GetItems( 1, new CancellationToken() );
			var correctIds = items.Select( p => p.ItemId ).ToList();
			var fakeIds = new List< int > { -1, 99999990, 99999991, 99999992, 99999993, 99999994, 99999995 };
			var allIds = correctIds.Union( fakeIds ).ToList();

			var existingItems = await this._service.GetExistingItemsIdsAsync( allIds, new CancellationToken() );
			var resultIds = existingItems.ToList();

			Assert.AreEqual( resultIds.Count, correctIds.Count );
			correctIds.Sort();
			resultIds.Sort();
			for (var i = 0; i < resultIds.Count; i++)
			{
				Assert.AreEqual( resultIds[ i ], correctIds[ i ] );
			}
		}
	}
}