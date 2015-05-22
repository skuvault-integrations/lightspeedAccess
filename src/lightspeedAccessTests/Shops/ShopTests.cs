using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess;
using lightspeedAccess.Models.Configuration;
using NUnit.Framework;

namespace lightspeedAccessTests.Shops
{
	class ShopTests
	{
		[Test]
		public void ShopTest()
		{
			var factory = new LightspeedFactory();
			var service = factory.CreateShopsService( new LightspeedConfig(797, "303f2b1c8400dff842a1376ce3370eb52f3c127b5e1777b723c4691141d7d900") );

			var shops = service.GetShops();
			Assert.Greater( shops.Count(), 0 );
		}

		[Test]
		public void ShopTest2()
		{
			var factory = new LightspeedFactory();
			var service = factory.CreateShopsService( new LightspeedConfig() );
			service.UpdateOnHandQuantity();
		}
	}
}
