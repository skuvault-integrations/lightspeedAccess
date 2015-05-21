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
			var service = factory.CreateShopsService( new LightspeedConfig() );

			var shops = service.GetShops();
			Assert.Greater( shops.Count(), 0 );
		}
	}
}
