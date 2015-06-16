﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LightspeedAccess.Misc;
using LightspeedAccess.Services;
using Netco.Extensions;

namespace lightspeedAccess
{
	class LightspeedAuthService : ILigthspeedAuthService
	{

		private readonly string _ligthspeedClientId;
		private readonly string _lightspeedClientSecret;
		private readonly string _redirectUri;

		private const string AuthTokenEndpoint = "https://cloud.merchantos.com/oauth/access_token.php";
		private const string TemporaryTokenEndpoint = "https://cloud.merchantos.com/oauth/authorize.php";

		public LightspeedAuthService( string lightspeedClientId, string lightspeedClientSecret, string redirectUri )
		{
			LightspeedLogger.Log.Debug( "Started LightspeedOrdersService" );
	
			this._lightspeedClientSecret = lightspeedClientSecret;
			this._ligthspeedClientId = lightspeedClientId;
			this._redirectUri = redirectUri;
		}

		private string ExecuteWebRequest( string temporaryToken ) 
		{
			LightspeedLogger.Log.Debug( "Creating get auth token request with a temporary token {0}", temporaryToken );

			var uri = new Uri( AuthTokenEndpoint );
			var request = ( HttpWebRequest ) WebRequest.Create( uri );

			var data = "client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}".FormatWith(
					_ligthspeedClientId, _lightspeedClientSecret, temporaryToken, _redirectUri
				);

			request.Method = WebRequestMethods.Http.Post;

			request.ContentLength = data.Length;

			using ( var stream = request.GetRequestStream() )
			{
				stream.Write( GetBytes(data), 0, data.Length );
			}
			LightspeedLogger.Log.Debug( "Request body created sucessfully, sending it to server", temporaryToken );

			var response = request.GetResponse();
			LightspeedLogger.Log.Debug( "Successfully got response from server, reading response stream");

			var reader = new StreamReader( response.GetResponseStream() );
			return reader.ReadToEnd();
		}

		private static byte[] GetBytes( string str )
		{
			byte[] bytes = new byte[ str.Length * sizeof( char ) ];
			System.Buffer.BlockCopy( str.ToCharArray(), 0, bytes, 0, bytes.Length );
			return bytes;
		}

		public string GetAuthToken( string accessToken )
		{
			return this.ExecuteWebRequest( accessToken );
		}

		public string GetAuthUrl()
		{
			return "{0}/?response_type=code&client_id={1}&scope=employee:all".FormatWith( TemporaryTokenEndpoint, this._ligthspeedClientId ); 
		} 
	}
}