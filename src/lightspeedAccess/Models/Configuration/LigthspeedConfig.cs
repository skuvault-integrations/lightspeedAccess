using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.Extensions;

namespace LightspeedAccess.Models.Configuration
{
	public class LightspeedConfig
	{
		public string Endpoint { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }

		public string LightspeedAuthToken { get; private set; }

		public static int DefaultTimeoutSeconds = 10;
		public readonly int TimeoutSeconds;

		public readonly string path = "https://api.merchantos.com/API/Account";

		public LightspeedConfig()
		{
			var testAccountID = 797;
			Endpoint = string.Format( "{0}/{1}/", path, testAccountID );
			Username = "imademo";
			Password = "thisismypass";
			TimeoutSeconds = DefaultTimeoutSeconds; 
		}

		public LightspeedConfig( int accountId, string authToken )
		{
			Endpoint = string.Format( "{0}/{1}/", path, accountId );
			LightspeedAuthToken = authToken;
			TimeoutSeconds = DefaultTimeoutSeconds; 
		}

		public LightspeedConfig(int timeoutSeconds)
		{
			var testAccountID = 797;
			Endpoint = string.Format( "{0}/{1}/", path, testAccountID );
			Username = "imademo";
			Password = "thisismypass";
			TimeoutSeconds = timeoutSeconds;
		}

		public LightspeedConfig( string authToken )
		{
			Endpoint = string.Format( "{0}/", path );
			LightspeedAuthToken = authToken;
			TimeoutSeconds = DefaultTimeoutSeconds; 
		}

		public override string ToString()
		{
			var str = "Lightspeed config: Endpoint = {0} ".FormatWith( this.Endpoint ) ;
			if ( this.LightspeedAuthToken != null ) { 
				str = str + "Authtoken: {0}".FormatWith( this.LightspeedAuthToken ); 
				return str;
			} 
			if ( this.Username != null && this.Password != null) {
				str = str + "No auth token (test mode, using basic auth) ";
				return str;
			}
			str = str + "No auth token (pre-auth mode)";
			return str;
		}
	}

}