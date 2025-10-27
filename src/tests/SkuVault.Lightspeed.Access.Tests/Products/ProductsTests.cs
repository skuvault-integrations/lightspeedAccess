using System.Linq;
using System.Threading;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace SkuVault.Lightspeed.Access.Tests.Products
{
	internal class ProductsTests : BaseTests
	{
		[ Explicit ]
		[ Test ]
		public void GetProductsAsync()
		{
			var service = GetProductsService();
			var cSource = new CancellationTokenSource();

			var products = service.GetProductsAsync( 1, cSource.Token ).GetAwaiter().GetResult();

			Assert.Greater( products.Count(), 0 );
			Assert.That(
				products.Count( x => x.DefaultVendorId != 0 ),
				Is.EqualTo( products.Count( x => !string.IsNullOrEmpty( x.DefaultVendorName ) ) ) );
			Assert.Greater( products.Count( x => x.Manufacturer != null ), 0 );
			Assert.Greater( products.Count( x => x.Description != null ), 0 );
		}

		private ILightspeedProductsService GetProductsService()
		{
			var provider = CreatePublicServiceProvider();
			var factory = provider.GetRequiredService<ILightspeedFactory>();
			return factory.CreateProductsService(_config, SyncRunContext);
		}
	}
}