using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightspeedAccess;
using LightspeedAccess.Models.Configuration;
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
			var credentials = new Credentials.TestsCredentials( @"..\..\Files\lightspeedCredentials.csv" );
			this._factory = new LightspeedFactory( credentials.ClientId, credentials.ClientSecret, "" );
			this._config = new LightspeedConfig( credentials.AccountId, credentials.AccessToken, credentials.RefreshToken );
		}

		[ Test ]
		public void OrderServiceTest()
		{
			var service = this._factory.CreateOrdersService( this._config );
			var endDate = DateTime.Now;
			var startDate = endDate.Subtract( new TimeSpan( 10000, 0, 0, 0 ) );

			var orders = service.GetOrders( startDate, endDate );

			Assert.Greater( orders.Count(), 0 );
		}

		[ Test ]
		public void OrderServiceTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );

			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();
			var orders = service.GetOrdersAsync( startDate, endDate, cSource.Token );

			orders.Wait();
			Assert.Greater( orders.Result.Count(), 0 );
		}

		[ Test ]
		public void SingleServiceThrottlerTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );
			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();

			for( int i = 0; i < 200; i++ )
			{
				var ordersTask = service.GetOrdersAsync( startDate, endDate, cSource.Token );
				ordersTask.Wait( cSource.Token );
			}

			Assert.Greater( 5, 0 );
		}

		[ Test ]
		public void MultipleServicesThrottlerTestAsync()
		{
			var service = _factory.CreateOrdersService( _config );
			var invService = _factory.CreateShopsService( _config );
			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );

			var cSource = new CancellationTokenSource();
			var itemsTask = invService.GetItems( 1, cSource.Token );
			itemsTask.Wait( cSource.Token );
			var item = itemsTask.Result.First();

			var tasks = new List< Task >();
			for( int i = 0; i < 100; i++ )
			{
				var ordersTask = service.GetOrdersAsync( startDate, endDate, cSource.Token );
				var itemUpdateTask = invService.UpdateOnHandQuantityAsync( item.ItemId, item.ItemShops[ 0 ].ShopId, item.ItemShops[ 0 ].ItemShopId, 10, cSource.Token );

				tasks.Add( ordersTask );
				tasks.Add( itemUpdateTask );
			}

			Task.WaitAll( tasks.ToArray() );

			Assert.Greater( 5, 0 );
		}

		[ Test ]
		public void SmokeTest()
		{
			var service = _factory.CreateLightspeedAuthService();
			var token = service.GetAuthByTemporyToken( "" );
			Console.WriteLine( "YOUR TOKEN IS: " + token );
			Assert.Greater( 1, 0 );
		}
	}
}