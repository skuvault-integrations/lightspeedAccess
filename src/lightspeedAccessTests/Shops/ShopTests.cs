using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lightspeedAccess;
using lightspeedAccess.Models.Configuration;
using NUnit.Framework;

namespace lightspeedAccessTests.Shops
{
	class ShopTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		[SetUp]
		public void Init()
		{
			_factory = new LightspeedFactory();
			_config = new LightspeedConfig();
		}

		[Test]
		public void GetShopsTest()
		{
			var service = _factory.CreateShopsService( _config );

			var shops = service.GetShops();
			Assert.Greater( shops.Count(), 0 );
		}

		[Test]
		public void GetShopsTestAsync()
		{
			var service = _factory.CreateShopsService( _config );

			var cSource = new CancellationTokenSource();

			var shops = service.GetShopsAsync( cSource.Token );
			shops.Wait();

			Assert.Greater( shops.Result.Count(), 0 );
		}

		[Test]
		public void PushToShopTest()
		{
			var service = _factory.CreateShopsService( _config );
			service.UpdateOnHandQuantity( 5, 172, 1 );
		}

		[Test]
		public void PushToShopAsyncTest()
		{
			var service = _factory.CreateShopsService( _config );
			var cSource = new CancellationTokenSource();
			var task = service.UpdateOnHandQuantityAsync( 5, 172, 1, cSource.Token );
			task.Wait();
		}

	}
}
