namespace SkuVault.Lightspeed.Access.Models.Configuration
{
	public class LightspeedConfig
	{
		private const string EmptyToken = "N/A";
		public string Endpoint{ get; }
		public string Username{ get; }
		public string Password{ get; }
		public int AccountId{ get; }
		public string LightspeedAccessToken{ get; internal set; }
		public string LightspeedRefreshToken { get; }

		public static int DefaultTimeoutSeconds = 10;
		public readonly int TimeoutSeconds;

		public static readonly string LightspeedUtcTimezoneCode = "%2D00:00";
		public static readonly string TimeFormat = "yyyy-MM-ddTHH:mm:ss";

		public readonly string path = "https://api.merchantos.com/API/Account";

		public string ClientId { get; }
		public string ClientSecret { get; }

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

		public LightspeedConfig( int accountId, string accessToken, string refreshToken, string clientId, string clientSecret )
		{
			this.AccountId = accountId;
			this.Endpoint = accountId > 0 ? $"{path}/{accountId}/" : $"{path}/";
			this.LightspeedAccessToken = string.IsNullOrWhiteSpace( accessToken ) ? EmptyToken : accessToken;
			this.LightspeedRefreshToken = refreshToken;
			this.TimeoutSeconds = DefaultTimeoutSeconds;
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
		}
	}
}