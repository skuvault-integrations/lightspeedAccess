using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess;
using lightspeedAccess.Models.Configuration;
using lightspeedAccess.Models.Order;
using lightspeedAccess.Models.Product;
using lightspeedAccess.Models.Request;
using lightspeedAccess.Services;
using NUnit.Framework;

namespace lightspeedAccessTests.Orders
{
	internal class OrderTests
	{
		[ Test ]
		public void ServiceTest()
		{
			var factory = new LightspeedFactory();
			var service = factory.CreateOrdersService( new LightspeedConfig() );
			var startDate = DateTime.ParseExact( "2015-05-16T16:30:27", "yyyy-MM-ddTHH:mm:ss",
				CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
			var endDate = startDate.Add( new TimeSpan( 10, 0, 0, 0 ) );

			var orders = service.GetOrders( startDate, endDate );
			Assert.Greater( orders.Count(), 0 );
		}
	}
}