using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Management;
using LightspeedAccess;
using LightspeedAccess.Models.Configuration;
using NUnit.Framework;

namespace lightspeedAccessTests.Products
{
	internal class ProductsTests
	{
		private LightspeedFactory _factory;
		private LightspeedConfig _config;

		[ SetUp ]
		public void Init()
		{
			var credentials = new Credentials.TestsCredentials(@"..\..\Files\lightspeedCredentials.csv");
			this._factory = new LightspeedFactory(credentials.ClientId, credentials.ClientSecret, "");
			this._config = new LightspeedConfig(credentials.AccountId, credentials.AccessToken, credentials.RefreshToken);
		}

		[ Test ]
		public void GetProductsAsync()
		{
			var service = _factory.CreateProductsService( _config );

			var cSource = new CancellationTokenSource();

			var products = service.GetProductsAsync( 1, cSource.Token ).GetAwaiter().GetResult();

			Assert.Greater( products.Count(), 0 );
			Assert.That(
				products.Count( x => x.DefaultVendorId != 0 ),
				Is.EqualTo( products.Count( x => !string.IsNullOrEmpty( x.DefaultVendorName ) ) ) );
		}
	}
}