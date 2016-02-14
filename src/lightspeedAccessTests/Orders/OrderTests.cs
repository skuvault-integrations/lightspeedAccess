using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

		private static LightspeedConfig GetConfig()
		{
			try {
				using ( StreamReader sr = new StreamReader( @"D:\lightspeedCredentials.txt" ) ) 
				{
					var accountId = sr.ReadLine();
					var token = sr.ReadLine();
					return new LightspeedConfig( Int32.Parse( accountId ), token );
				}
			}
			catch ( Exception e )
			{
				return new LightspeedConfig();
			}
		}

		[ SetUp ]
		public void Init()
		{
			this._factory = new LightspeedFactory( "", "", "" );
			this._config = GetConfig();
		}

		[ Test ]
		public void OrderServiceTest()
		{
			var service = this._factory.CreateOrdersService( this._config );
			var endDate = DateTime.Now;
			var startDate = endDate.Subtract( new TimeSpan( 10000, 0, 0, 0 ) );

			var ordersTask = service.GetOrdersAsync( startDate, endDate, CancellationToken.None );

			Task.WaitAll( ordersTask );
			Assert.Greater( ordersTask.Result.Count(), 0 );
		}

		[ Test ]
		public void OrderServiceTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );

			var startDate = DateTime.ParseExact( "2015-06-16T15:40:03", "yyyy-MM-ddTHH:mm:ss",
				CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
			var endDate = startDate.Add( new TimeSpan( 10, 0, 0, 0 ) );

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