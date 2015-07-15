using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Order;
using LightspeedAccess.Models.Product;
using LightspeedAccess.Models.Request;
using LightspeedAccess.Services;
using NUnit.Framework;

namespace lightspeedAccessTests.Orders
{
	internal class OrderTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		[ SetUp ]
		public void Init()
		{
			_factory = new LightspeedFactory( "", "", "" );
			_config = new LightspeedConfig();
		}

		[ Test ]
		public void OrderServiceTest()
		{
			var service = _factory.CreateOrdersService( _config );
			var startDate = DateTime.ParseExact( "2015-06-16T16:30:27", "yyyy-MM-ddTHH:mm:ss",
				CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
			var endDate = startDate.Add( new TimeSpan( 10000, 0, 0, 0 ) );

			var orders = service.GetOrders( startDate, endDate );

			Assert.Greater( orders.Count(), 0 );
		}

		[ Test ]
		public void OrderServiceTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );
			//			Retrieving all Lightspeed sales from '06/20/2015 15:40:03' to '06/20/2015 15:48:
			//13'
			var startDate = DateTime.ParseExact( "2015-06-16T15:40:03", "yyyy-MM-ddTHH:mm:ss",
				CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
			var endDate = startDate.Add( new TimeSpan( 10, 0, 0, 0 ) );
			//			var endDate = DateTime.ParseExact( "2015-06-20T15:48:13", "yyyy-MM-ddTHH:mm:ss",
			//				CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
			var cSource = new CancellationTokenSource();
			var orders = service.GetOrdersAsync( startDate, endDate, cSource.Token );

			orders.Wait();
			Assert.Greater( orders.Result.Count(), 0 );
		}

		[ Test ]
		public void SmokeTest()
		{
			var service = _factory.CreateLightspeedAuthService();
			var token = service.GetAuthToken( "" );
			Console.WriteLine( "YOUR TOKEN IS: " + token );
			Assert.Greater( 1, 0 );
		}
	}
}