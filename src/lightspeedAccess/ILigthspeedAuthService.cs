using lightspeedAccess.Models.Auth;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccess
{
	public interface ILigthspeedAuthService
	{
		AuthResult GetAuthByTemporyToken( string temporyToken, SyncRunContext syncRunContext );
		string GetAuthUrl();
	}
}