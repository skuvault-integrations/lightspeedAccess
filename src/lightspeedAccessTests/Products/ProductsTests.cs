using System;
using System.Linq;
using System.Threading;
using lightspeedAccess;
using lightspeedAccess.Models.Configuration;
using NUnit.Framework;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccessTests.Products
{
	internal class ProductsTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		private static SyncRunContext SyncRunContext => new SyncRunContext( 1, 2, Guid.NewGuid().ToString() );

		[ SetUp ]
		public void Init()
		{
			var credentials = new Credentials.TestsCredentials( @"..\..\Files\lightspeedCredentials.csv" );
			this._factory = new LightspeedFactory( credentials.ClientId, credentials.ClientSecret, "" );
			this._config = new LightspeedConfig( credentials.AccountId, credentials.AccessToken, credentials.RefreshToken );
		}

		[ Test ]
		public void GetProductsAsync()
		{
			var service = _factory.CreateProductsService( _config, SyncRunContext );

			var cSource = new CancellationTokenSource();

			var products = service.GetProductsAsync( 1, cSource.Token ).GetAwaiter().GetResult();

			Assert.Greater( products.Count(), 0 );
			Assert.That(
				products.Count( x => x.DefaultVendorId != 0 ),
				Is.EqualTo( products.Count( x => !string.IsNullOrEmpty( x.DefaultVendorName ) ) ) );
			Assert.Greater( products.Count( x => x.Manufacturer != null ), 0 );
			Assert.Greater( products.Count( x => x.Description != null ), 0 );
		}
	}
}