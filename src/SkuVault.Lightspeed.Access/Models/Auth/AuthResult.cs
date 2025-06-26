namespace SkuVault.Lightspeed.Access.Models.Auth
{
	public class AuthResult
	{
		public string AccessToken { get; private set; }
		public string RefreshToken { get; private set; }

		public AuthResult( string accessToken, string refreshToken )
		{
			this.AccessToken = accessToken;
			this.RefreshToken = refreshToken;
		}
	}
}
