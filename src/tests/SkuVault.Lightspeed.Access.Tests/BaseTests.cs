using System;
using SkuVault.Lightspeed.Access.Models.Configuration;
using NUnit.Framework;
using SkuVault.Integrations.Core.Common;
using NSubstitute;
using SkuVault.Integrations.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using SkuVault.Lightspeed.Access.Extensions;
using Microsoft.Extensions.Logging;

namespace SkuVault.Lightspeed.Access.Tests
{
	internal class BaseTests
	{
		protected LightspeedFactory _factory;
		protected LightspeedConfig _config;
		protected static SyncRunContext SyncRunContext => new SyncRunContext( 1, 2, Guid.NewGuid().ToString() );

		[ SetUp ]
		public void Init()
		{
			var credentials = new Credentials.TestsCredentials( @"..\..\Files\lightspeedCredentials.csv" );
			IIntegrationLogger logger = Substitute.For<IIntegrationLogger>();
			_factory = new LightspeedFactory( logger );
			_config = new LightspeedConfig( credentials.AccountId, credentials.AccessToken, credentials.RefreshToken,
				credentials.ClientId, credentials.ClientSecret );
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