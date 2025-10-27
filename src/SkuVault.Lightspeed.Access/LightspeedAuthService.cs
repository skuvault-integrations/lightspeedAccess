using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Helpers;
using SkuVault.Lightspeed.Access.Models.Auth;
using SkuVault.Integrations.Core.Logging;
using Microsoft.Extensions.Logging;

namespace SkuVault.Lightspeed.Access
{
	public class LightspeedAuthService: ILigthspeedAuthService
	{
		private const string AuthTokenEndpoint = "https://cloud.merchantos.com/oauth/access_token.php";
		private const string TemporaryTokenEndpoint = "https://cloud.merchantos.com/oauth/authorize.php";
		private readonly IIntegrationLogger _logger;
		private readonly string _ligthspeedClientId;
		private readonly string _lightspeedClientSecret;
		private readonly SyncRunContext _syncRunContext;
		private const string CallerType = nameof(LightspeedOrdersService);

		private enum RequestType { GetAuthorizationCode, RefreshToken }

		public LightspeedAuthService( string lightspeedClientId, string lightspeedClientSecret, SyncRunContext syncRunContext, IIntegrationLogger logger )
		{
			_lightspeedClientSecret = lightspeedClientSecret;
			_ligthspeedClientId = lightspeedClientId;
			_logger = logger;
			_syncRunContext = syncRunContext;
		}

		public AuthResult GetAuthByTemporyToken( string temporyToken )
		{
			return this.GetAuthInfo( temporyToken, RequestType.GetAuthorizationCode );
		}

		public string GetAuthUrl()
		{
			return $"{TemporaryTokenEndpoint}/?response_type=code&client_id={_ligthspeedClientId}&scope=employee:register%20employee:inventory%20employee:admin_shops%20employee:customers";
		}

		public string GetNewAccessToken( string refreshToken )
		{
			var authResult = this.GetAuthInfo( refreshToken, RequestType.RefreshToken );
			return authResult.AccessToken;
		}

		internal static bool IsUnauthorizedException( Exception ex )
		{
			if ( ex is not WebException webException )
				return false;

			return IsUnauthorizedException( webException );
		}

		internal static bool IsUnauthorizedException( WebException ex )
		{
			if ( ex.Response is not HttpWebResponse response )
				return false;

			return response.StatusCode == HttpStatusCode.Unauthorized;
		}

		private AuthResult GetAuthInfo( string token, RequestType requestType )
		{
			var sanitizedToken = TokenSanitizer.SanitizeToken( token );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Creating get auth token request with a token '{Token}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetAuthInfo),
				sanitizedToken );

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
			}

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "Request body created successfully, sending it to server: '{Data}', Token: '{SanitizedToken}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetAuthInfo),
				data,
				sanitizedToken );

			var response = request.GetResponse();
			var reader = new StreamReader( response.GetResponseStream() );
			var responseJson = reader.ReadToEnd();

			var jsonDictionary = JsonConvert.DeserializeObject< Dictionary< string, object > >( responseJson );
			var accessToken = ( string )jsonDictionary[ "access_token" ];
			var refreshToken = requestType == RequestType.GetAuthorizationCode ? ( string )jsonDictionary[ "refresh_token" ] : string.Empty;

			var sanitizedAccessToken = TokenSanitizer.SanitizeToken( accessToken );

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[End]: Deserialization completed successfully, your token is '{SanitizedAccessToken}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				_syncRunContext.TenantId,
				_syncRunContext.ChannelAccountId,
				_syncRunContext.CorrelationId,
				CallerType,
				nameof(GetAuthInfo),
				sanitizedAccessToken );

			return new AuthResult( accessToken, refreshToken );
		}
	}
}