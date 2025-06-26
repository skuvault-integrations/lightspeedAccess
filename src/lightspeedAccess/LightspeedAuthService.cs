using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using lightspeedAccess.Helpers;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Auth;
using Newtonsoft.Json;
using SkuVault.Integrations.Core.Common;

namespace lightspeedAccess
{
	public class LightspeedAuthService: ILigthspeedAuthService
	{
		private readonly string _ligthspeedClientId;
		private readonly string _lightspeedClientSecret;

		private const string AuthTokenEndpoint = "https://cloud.merchantos.com/oauth/access_token.php";
		private const string TemporaryTokenEndpoint = "https://cloud.merchantos.com/oauth/authorize.php";
		private const string CallerType = nameof(LightspeedAuthService);

		private enum RequestType { GetAuthorizationCode, RefreshToken }

		public LightspeedAuthService( string lightspeedClientId, string lightspeedClientSecret )
		{
			this._lightspeedClientSecret = lightspeedClientSecret;
			this._ligthspeedClientId = lightspeedClientId;
		}

		public AuthResult GetAuthByTemporyToken( string temporyToken, SyncRunContext syncRunContext )
		{
			return this.GetAuthInfo( temporyToken, RequestType.GetAuthorizationCode, syncRunContext );
		}

		public string GetAuthUrl()
		{
			return $"{TemporaryTokenEndpoint}/?response_type=code&client_id={_ligthspeedClientId}&scope=employee:register%20employee:inventory%20employee:admin_shops%20employee:customers";
		}

		internal string GetNewAccessToken( string refreshToken, SyncRunContext syncRunContext )
		{
			var authResult = this.GetAuthInfo( refreshToken, RequestType.RefreshToken, syncRunContext );
			return authResult.AccessToken;
		}

		internal static bool IsUnauthorizedException( Exception ex )
		{
			var webException = ex as WebException;
			if( webException == null )
				return false;

			return IsUnauthorizedException( webException );
		}

		internal static bool IsUnauthorizedException( WebException ex )
		{
			var response = ex.Response as HttpWebResponse;
			if( response == null )
				return false;

			return response.StatusCode == HttpStatusCode.Unauthorized;
		}

		private AuthResult GetAuthInfo( string token, RequestType requestType, SyncRunContext syncRunContext )
		{
			var sanitizedToken = TokenSanitizer.SanitizeToken( token );
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Creating get auth token request with a token {sanitizedToken}" );

			var uri = new Uri( AuthTokenEndpoint );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			var data = $"client_id={this._ligthspeedClientId}&client_secret={this._lightspeedClientSecret}";

			switch ( requestType )
			{
				case RequestType.GetAuthorizationCode:
					data += $"&code={token}&grant_type=authorization_code";
					break;
				case RequestType.RefreshToken:
					data += $"&refresh_token={token}&grant_type=refresh_token";
					break;
			}

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = WebRequestMethods.Http.Post;

			request.ContentLength = data.Length;

			using( StreamWriter stOut = new StreamWriter( request.GetRequestStream(), System.Text.Encoding.ASCII ) )
			{
				stOut.Write( data );
				stOut.Close();
			}

			LightspeedLogger.Debug( syncRunContext, CallerType, $"Request body created sucessfully, sending it to server: {data}, Token:{sanitizedToken}" );

			var response = request.GetResponse();
			LightspeedLogger.Debug( syncRunContext, CallerType, "Successfully got response from server, reading response stream" );

			var reader = new StreamReader( response.GetResponseStream() );

			var responseJson = reader.ReadToEnd();

			LightspeedLogger.Debug( syncRunContext, CallerType, "Response stream reading complete. Starting deserialization" );
			var jsonDictionary = JsonConvert.DeserializeObject< Dictionary< string, object > >( responseJson );
			var accessToken = ( string )jsonDictionary[ "access_token" ];
			var refreshToken = requestType == RequestType.GetAuthorizationCode ? ( string )jsonDictionary[ "refresh_token" ] : string.Empty;

			var sanitizedAccessToken = TokenSanitizer.SanitizeToken( accessToken );
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Deserialization completed successfully, your token is {sanitizedAccessToken}" );
			return new AuthResult( accessToken, refreshToken );
		}
	}
}