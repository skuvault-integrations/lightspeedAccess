using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Models.Auth;

namespace SkuVault.Lightspeed.Access
{
	public interface ILigthspeedAuthService
	{
		AuthResult GetAuthByTemporyToken( string temporyToken, SyncRunContext syncRunContext );
		string GetAuthUrl();
	}
}