using SkuVault.Lightspeed.Access.Models.Auth;

namespace SkuVault.Lightspeed.Access
{
	public interface ILigthspeedAuthService
	{
		AuthResult GetAuthByTemporyToken( string temporyToken );
		string GetAuthUrl();
		string GetNewAccessToken( string refreshToken );
	}
}