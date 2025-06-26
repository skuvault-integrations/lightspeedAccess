using SkuVault.Integrations.Core.Common;

namespace lightspeedAccess
{
	public interface IAccountService
	{
		int GetAccountId( SyncRunContext syncRunContext );
	}
}