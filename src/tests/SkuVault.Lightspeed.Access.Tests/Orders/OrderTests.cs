using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkuVault.Lightspeed.Access.Models.Configuration;
using NUnit.Framework;
using SkuVault.Integrations.Core.Common;
using NSubstitute;
using SkuVault.Integrations.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using SkuVault.Lightspeed.Access.Extensions;
using Microsoft.Extensions.Logging;

namespace SkuVault.Lightspeed.Access.Tests.Orders
{
	internal class OrderTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;
		private static SyncRunContext SyncRunContext => new SyncRunContext( 1, 2, Guid.NewGuid().ToString() );

		[ SetUp ]
		public void Init()
		{
			var credentials = new Credentials.TestsCredentials( @"..\..\Files\lightspeedCredentials.csv" );
			IIntegrationLogger logger = Substitute.For<IIntegrationLogger>();
			_factory = new LightspeedFactory( logger );
			_config = new LightspeedConfig( credentials.AccountId, credentials.AccessToken, credentials.RefreshToken,
				credentials.ClientId, credentials.ClientSecret );
		}

		[ Explicit ]
		[ Test ]
		public void OrderServiceTest()
		{
			var service = GetOrdersService();
			var endDate = DateTime.Now;
			var startDate = endDate.Subtract( new TimeSpan( 10000, 0, 0, 0 ) );

			var orders = service.GetOrders( startDate, endDate );

			Assert.Greater( orders.Count(), 0 );
		}

		[ Explicit ]
		[ Test ]
		public async Task OrderServiceTestAsync()
		{
			// arrange
			var service = GetOrdersService();
			var endDate = DateTime.Now;
			var startDate = endDate.AddMonths( -6 );
			var cSource = new CancellationTokenSource();

			// act
			var orders = await service.GetOrdersAsync( startDate, endDate, cSource.Token );

			// assert
			Assert.That( orders.Count(), Is.GreaterThan( 0 ) );
		}

		[ Explicit ]
		[ Test ]
		public void SingleServiceThrottlerTestAsync()
		{
			var service = GetOrdersService();
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

		[ Explicit ]
		[ Test ]
		public void MultipleServicesThrottlerTestAsync()
		{
			var service = GetOrdersService();
			var invService = _factory.CreateShopsService( _config, SyncRunContext );
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

		[ Explicit ]
		[ Test ]
		public void SmokeTest()
		{
			var service = _factory.CreateLightspeedAuthService( _config, SyncRunContext );
			var token = service.GetAuthByTemporyToken( "" );
			Console.WriteLine( "YOUR TOKEN IS: " + token );
			Assert.Greater( 1, 0 );
		}

		private ILightspeedOrdersService GetOrdersService()
		{
			var provider = CreatePublicServiceProvider();
			var factory = provider.GetRequiredService<ILightspeedFactory>();
			return factory.CreateOrdersService( _config, SyncRunContext );
		}

		protected static IServiceProvider CreatePublicServiceProvider()
		{
			var serviceCollection = new ServiceCollection()
				.AddLightspeedServices(builder =>
				{
					builder.SetMinimumLevel(LogLevel.Information);
				});
			return serviceCollection.BuildServiceProvider();
		}

		protected static SyncRunContext CreateSyncRunContext(long tenantId = 1, long? channelAccountId = null) =>
			new SyncRunContext(tenantId, channelAccountId, Guid.NewGuid().ToString());
	}
}