namespace SkuVault.Lightspeed.Access.Models.Configuration
{
	public class LightspeedConfig
	{
		private const string EmptyToken = "N/A";
		public string Endpoint{ get; private set; }
		public string Username{ get; private set; }
		public string Password{ get; private set; }

		public int AccountId{ get; private set; }

		public string LightspeedAccessToken{ get; internal set; }
		public string LightspeedRefreshToken{ get; private set; }

		public static int DefaultTimeoutSeconds = 10;
		public readonly int TimeoutSeconds;

		public static readonly string LightspeedUtcTimezoneCode = "%2D00:00";
		public static readonly string TimeFormat = "yyyy-MM-ddTHH:mm:ss";

		public readonly string path = "https://api.merchantos.com/API/Account";

		public LightspeedConfig(): this( DefaultTimeoutSeconds )
		{
		}

		public LightspeedConfig( int timeoutSeconds )
		{
			var testAccountID = 797;
			this.Endpoint = string.Format( "{0}/{1}/", path, testAccountID );
			this.Username = "imademo";
			this.Password = "thisismypass";
			this.TimeoutSeconds = timeoutSeconds;
		}

		public LightspeedConfig( int accountId, string accessToken, string refreshToken )
		{
			this.AccountId = accountId;
			this.Endpoint = string.Format( "{0}/{1}/", path, accountId );
			this.LightspeedAccessToken = string.IsNullOrWhiteSpace( accessToken ) ? EmptyToken : accessToken;
			this.LightspeedRefreshToken = refreshToken;
			this.TimeoutSeconds = DefaultTimeoutSeconds;
		}

		public LightspeedConfig( string accessToken, string refreshToken )
		{
			this.Endpoint = string.Format( "{0}/", path );
			this.LightspeedAccessToken = string.IsNullOrWhiteSpace( accessToken ) ? EmptyToken : accessToken;
			this.LightspeedRefreshToken = refreshToken;
			this.TimeoutSeconds = DefaultTimeoutSeconds;
		}

		public override string ToString()
		{
			var str = $"Lightspeed config: Endpoint = {this.Endpoint} ";
			if( !string.IsNullOrWhiteSpace( this.LightspeedRefreshToken ) )
			{
				str = str + $"RefreshToken: {this.LightspeedRefreshToken} ";
			}
			if( !string.IsNullOrWhiteSpace( this.LightspeedAccessToken ) )
			{
				str = str + $"Authtoken: {this.LightspeedAccessToken}";
				return str;
			}
			if( this.Username != null && this.Password != null )
			{
				str = str + "No auth token (test mode, using basic auth) ";
				return str;
			}
			str = str + "No auth token (pre-auth mode)";
			return str;
		}
	}
}