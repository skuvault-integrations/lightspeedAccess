using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using LightspeedAccess.Misc;
using System.Web.Script.Serialization;
using lightspeedAccess.Models.Auth;
using Netco.Extensions;

namespace lightspeedAccess
{
	public class LightspeedAuthService: ILigthspeedAuthService
	{
		private readonly string _ligthspeedClientId;
		private readonly string _lightspeedClientSecret;

		private const string AuthTokenEndpoint = "https://cloud.merchantos.com/oauth/access_token.php";
		private const string TemporaryTokenEndpoint = "https://cloud.merchantos.com/oauth/authorize.php";

		private enum RequestType { GetAuthorizationCode, RefreshToken }

		public LightspeedAuthService( string lightspeedClientId, string lightspeedClientSecret )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedOrdersService" );

			this._lightspeedClientSecret = lightspeedClientSecret;
			this._ligthspeedClientId = lightspeedClientId;
		}

		public AuthResult GetAuthByTemporyToken( string temporyToken )
		{
			return this.GetAuthInfo( temporyToken, RequestType.GetAuthorizationCode );
		}

		public string GetAuthUrl()
		{
			return "{0}/?response_type=code&client_id={1}&scope=employee:register%20employee:inventory%20employee:admin_shops%20employee:customers".FormatWith( TemporaryTokenEndpoint, this._ligthspeedClientId );
		}

		internal string GetNewAccessToken( string refreshToken )
		{
			var authResult = this.GetAuthInfo( refreshToken, RequestType.RefreshToken );
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

		private AuthResult GetAuthInfo( string token, RequestType requestType )
		{
			LightspeedLogger.Log.Debug( "Creating get auth token request with a token {0}", token );

			var uri = new Uri( AuthTokenEndpoint );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			var data = "client_id={0}&client_secret={1}".FormatWith( this._ligthspeedClientId, this._lightspeedClientSecret );

			switch( requestType )
			{
				case RequestType.GetAuthorizationCode:
					data += "&code={0}&grant_type=authorization_code".FormatWith( token );
					break;
				case RequestType.RefreshToken:
					data += "&refresh_token={0}&grant_type=refresh_token".FormatWith( token );
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

			LightspeedLogger.Log.Debug( "Request body created sucessfully, sending it to server: " + data, token );

			var response = request.GetResponse();
			LightspeedLogger.Log.Debug( "Successfully got response from server, reading response stream" );

			var reader = new StreamReader( response.GetResponseStream() );

			var responseJson = reader.ReadToEnd();

			LightspeedLogger.Log.Debug( "Response stream reading complete. Starting deserialization" );
			var serializer = new JavaScriptSerializer();
			var jsonDictionary = ( IDictionary< string, object > )serializer.DeserializeObject( responseJson );
			string accessToken = ( string )jsonDictionary[ "access_token" ];
			string refreshToken = requestType == RequestType.GetAuthorizationCode ? ( string )jsonDictionary[ "refresh_token" ] : String.Empty;

			LightspeedLogger.Log.Debug( "Deserialization completed successfully, your token is {1}", accessToken );
			return new AuthResult( accessToken, refreshToken );
		}
	}
}