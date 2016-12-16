using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess;
using LightspeedAccess.Models.Configuration;
using NUnit.Framework;

namespace lightspeedAccessTests.Shops
{
	internal class ShopTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		[ SetUp ]
		public void Init()
		{
			_factory = new LightspeedFactory( "n/a", "n/a", "n/a" );
			_config = new LightspeedConfig( 767 );
		}

		[ Test ]
		public void GetShopsTest()
		{
			var service = _factory.CreateShopsService( _config );

			var shops = service.GetShops();
			Assert.Greater( shops.Count(), 0 );
		}

		[ Test ]
		public void GetShopsTestAsync()
		{
			var service = _factory.CreateShopsService( _config );

			var cSource = new CancellationTokenSource();

			var shops = service.GetShopsAsync( cSource.Token );
			shops.Wait();

			Assert.Greater( shops.Result.Count(), 0 );
		}

		[ Test ]
		public void PushToShopTest()
		{
			var service = _factory.CreateShopsService( _config );
			service.UpdateOnHandQuantity( 5, 172, 1, 1 );
		}

		[ Test ]
		public void PushToShopAsyncTest()
		{
			var service = _factory.CreateShopsService( _config );
			var cSource = new CancellationTokenSource();
			var task = service.UpdateOnHandQuantityAsync( 5, 172, 1, 1, cSource.Token );
			task.Wait();
		}

		//210000000007

		[ Test ]
		public void GetItemsAsyncTest()
		{
			var service = _factory.CreateShopsService( _config );
			var cSource = new CancellationTokenSource();
			var task = service.GetItems( new List< String > { "210000000007" }, cSource.Token );
			task.Wait();
			Assert.Greater( task.Result.Count, 0 );
		}

		[ Test ]
		public void GetItemFromSHopsAsyncTest()
		{
			var service = _factory.CreateShopsService( _config );
			var cSource = new CancellationTokenSource();
			var task = service.GetItems( 1, cSource.Token );
			task.Wait();
			Assert.Greater( task.Result.Count(), 0 );
		}

	}
}