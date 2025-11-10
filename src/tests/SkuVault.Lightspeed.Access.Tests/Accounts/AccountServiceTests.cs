using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SkuVault.Lightspeed.Access.Models.Configuration;

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
			var config = new LightspeedConfig(0, _credentials.AccessToken, _credentials.RefreshToken,
				_credentials.ClientId, _credentials.ClientSecret);
			return factory.CreateAccountsService(config, SyncRunContext);
		}
	}
}