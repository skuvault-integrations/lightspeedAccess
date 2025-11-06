using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace SkuVault.Lightspeed.Access.Tests.Account
{
	internal class AccountServiceTests : BaseTests
	{
		[ Explicit ]
		[ Test ]
		public void GetAccountId()
		{
			// Arrange
			var service = GetAccountService();

			// Act
			var accountId = service.GetAccountId();

			// Assert
			Assert.That(accountId, Is.EqualTo(TestConstants.SandboxAccountId));
		}

		private IAccountService GetAccountService()
		{
			var provider = CreatePublicServiceProvider();
			var factory = provider.GetRequiredService<ILightspeedFactory>();
			return factory.CreateAccountsService(_config, SyncRunContext);
		}
	}
}