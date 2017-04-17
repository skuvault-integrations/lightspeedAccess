using lightspeedAccess.Models.Auth;

namespace lightspeedAccess
{
	public interface ILigthspeedAuthService
	{
		AuthResult GetAuthByTemporyToken( string temporyToken );
		string GetAuthUrl();
	}
}