using SkuVault.Integrations.Core.Common;

namespace SkuVault.Lightspeed.Access
{
	public interface IAccountService
	{
		int GetAccountId( SyncRunContext syncRunContext );
	}
}