using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess
{
	public interface ILigthspeedAuthService
	{
		string GetAuthToken( string accessToken );
		string GetAuthUrl();
	}
}