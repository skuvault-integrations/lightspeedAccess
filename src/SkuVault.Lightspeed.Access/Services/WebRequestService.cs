using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access;
using SkuVault.Lightspeed.Access.Helpers;
using SkuVault.Lightspeed.Access.Misc;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Models.Configuration;

namespace SkuVault.Lightspeed.Access.Services
{
	internal class WebRequestService
	{
		private readonly LightspeedConfig _config;
		private readonly ThrottlerAsync _throttler;
		private readonly LightspeedAuthService _authService;
		private const string CallerType = nameof(WebRequestService);

		public WebRequestService( LightspeedConfig config, ThrottlerAsync throttler, LightspeedAuthService authService )
		{
			this._config = config;
			this._throttler = throttler;
			this._authService = authService;
		}

		private static void ManageRequestBody( LightspeedRequest request, ref HttpWebRequest webRequest, SyncRunContext syncRunContext )
		{
			var body = request.GetBody();

			if( body == null )
				return;
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Creating request body from stream for request {request}" );

			webRequest.Method = "PUT";

			webRequest.ContentType = "text/xml";

			body.Seek( 0, SeekOrigin.Begin );

			using ( var sr = new StreamReader( body ) )
			{
				var s = sr.ReadToEnd();

				LightspeedLogger.Debug( syncRunContext, CallerType, $"Created request body for request {request} : {s}" );

				webRequest.ContentLength = s.Length;
				using( var dataStream = webRequest.GetRequestStream() )
				{
					var bytes = Encoding.UTF8.GetBytes( s );
					foreach ( var singleByte in bytes )
					{
						dataStream.WriteByte( singleByte );
					}

					dataStream.Close();
				}
			}
		}

		public T GetResponse< T >( LightspeedRequest request, SyncRunContext syncRunContext )
		{
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Making request {request} to lightspeed server" );

			var webRequestAction = new Func< WebResponse >( () =>
				{
					var webRequest = this.CreateHttpWebRequest( this._config.Endpoint + request.GetUri(), syncRunContext );
					ManageRequestBody( request, ref webRequest, syncRunContext );
					try
					{
						return webRequest.GetResponse();
					}
					catch( WebException ex )
					{
						LogRequestFailure( ex, request, webRequest, syncRunContext );
						if( LightspeedAuthService.IsUnauthorizedException( ex ) )
						{
							this.RefreshSession( syncRunContext );
						}
						throw;
					}
				}
			);

			using( var response = ActionPolicies.SubmitPolicy( syncRunContext ).Execute( () => webRequestAction() ) )
			{
				var stream = response.GetResponseStream();

				LightspeedLogger.Debug( syncRunContext, CallerType, $"Got response from server for request {request}, starting deserialization" );
				var deserializer = new XmlSerializer( typeof( T ) );
				var result = ( T ) deserializer.Deserialize( stream );
				LightspeedLogger.Debug( syncRunContext, CallerType, $"Successfylly deserialized response for request {request}. Response: {response.ToJson()}" );

				var possibleAdditionalResponses = this.IterateThroughPagination( request, result, syncRunContext );

				if( result is IPaginatedResponse aggregatedResult )
				{
					LightspeedLogger.Debug( syncRunContext, CallerType, $"Aggregating paginated results for request {request}" );
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		public async Task< T > GetResponseAsync< T >( LightspeedRequest request, SyncRunContext syncRunContext, CancellationToken ctx )
		{
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Making request {request} to lightspeed server" );
			var webRequestAction = new Func< Task< WebResponse > >(
				async () =>
				{
					var requestDelegate = this.CreateHttpWebRequest( this._config.Endpoint + request.GetUri(), syncRunContext );
					ManageRequestBody( request, ref requestDelegate, syncRunContext );
					try
					{
						return await requestDelegate.GetResponseAsync();
					}
					catch ( WebException ex )
					{
						LogRequestFailure( ex, request, requestDelegate, syncRunContext );
						if( LightspeedAuthService.IsUnauthorizedException( ex ) )
						{
							this.RefreshSession( syncRunContext );
						}
						if( IsItemNotFound( request, ex ) )
						{
							return null;
						}

						// PBL-9316: We agreed do not throw sync on bad request error for a specific item
						if( IsBadRequestException( ex ) )
						{
							return null;
						}

						throw;
					}
				} );

			using ( var response = await ( this.GetWrappedAsyncResponse( webRequestAction, syncRunContext, ctx ) ) )
			{
				if( response == null )
					return default( T );

				var stream = response.GetResponseStream();

				LightspeedLogger.Debug( syncRunContext, CallerType,
					$"Got response from server for request {request}, starting deserialization" );
				var deserializer =
					new XmlSerializer( typeof( T ) );

				var result = ( T ) deserializer.Deserialize( stream );

				LightspeedLogger.Debug( syncRunContext, CallerType,
					$"Successfully deserialized response for request {request}. Response: {result.ToJson()}" );
				var possibleAdditionalResponses = await this.IterateThroughPaginationAsync( request, result, syncRunContext, ctx );

				if ( result is IPaginatedResponse aggregatedResult )
				{
					LightspeedLogger.Debug( syncRunContext, CallerType,
						$"Aggregating paginated results for request {request}" );
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		private void RefreshSession( SyncRunContext syncRunContext )
		{
			this._config.LightspeedAccessToken = this._authService.GetNewAccessToken( this._config.LightspeedRefreshToken, syncRunContext );
		}

		private static bool IsItemNotFound( LightspeedRequest request, WebException ex )
		{
			if( !( request is GetItemRequest || request is UpdateOnHandQuantityRequest ) || ex.Status != WebExceptionStatus.ProtocolError )
			{
				return false;
			}

			var response = ex.Response as HttpWebResponse;
			if( response == null )
				return false;

			return response.StatusCode == HttpStatusCode.NotFound;
		}

		internal static bool IsBadRequestException( Exception exception )
		{
			var webException = exception as WebException;
			if ( webException == null )
				return false;

			var response = webException.Response as HttpWebResponse;
			if ( response == null )
				return false;

			return response.StatusCode == HttpStatusCode.BadRequest;
		}

		private static bool NeedToIterateThroughPagination< T >( T response, LightspeedRequest r )
		{
			var paginatedResponse = response as IPaginatedResponse;
			var requestWithPagination = r as IRequestPagination;
			return ( paginatedResponse != null && requestWithPagination != null &&
			         paginatedResponse.GetCount() > requestWithPagination.GetLimit() );
		}

		private List< T > IterateThroughPagination< T >( LightspeedRequest r, T response, SyncRunContext syncRunContext )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, r ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )r;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			LightspeedLogger.Debug( syncRunContext, CallerType,
				$"Response for request {r} was paginated, started iterating the remaining pages..." );

			var numPages = paginatedResponse.GetCount() / paginatedRequest.GetLimit() + 1;

			LightspeedLogger.Debug( syncRunContext, CallerType,
				$"Expected number of pages for request {r} : {numPages}" );
			for( var pageNum = 1; pageNum < numPages; pageNum++ )
			{
				LightspeedLogger.Debug( syncRunContext, CallerType,
					$"Processing page {numPages} for request {r}..." );
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( this.GetResponse< T >( r, syncRunContext ) );
			}

			return additionalResponses;
		}

		private async Task< List< T > > IterateThroughPaginationAsync< T >( LightspeedRequest r, T response, SyncRunContext syncRunContext, CancellationToken ctx )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, r ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )r;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			var numPages = paginatedResponse.GetCount() / paginatedRequest.GetLimit() + 1;

			LightspeedLogger.Debug( syncRunContext, CallerType,
				$"Expected number of pages for request {r} : {numPages}" );
			for( var pageNum = 1; pageNum < numPages; pageNum++ )
			{
				LightspeedLogger.Debug( syncRunContext, CallerType,
					$"Processing page {pageNum} / {numPages} for request {r}..." );
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( await this.GetResponseAsync< T >( r, syncRunContext, ctx ) );
			}

			return additionalResponses;
		}

		private HttpWebRequest CreateHttpWebRequest( string url, SyncRunContext syncRunContext )
		{
			LightspeedLogger.Debug( syncRunContext, CallerType, $"Composed lightspeed request URL: {url}" );
			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			request.Headers[ HttpRequestHeader.Authorization ] = this.CreateAuthenticationHeader( syncRunContext );
			request.Timeout = this._config.TimeoutSeconds * 1000;

			return request;
		}

		private string CreateAuthenticationHeader( SyncRunContext syncRunContext )
		{
			if( !string.IsNullOrWhiteSpace( this._config.LightspeedAccessToken ) )
			{
				return string.Concat( "Bearer ", this._config.LightspeedAccessToken );
			}
			
			LightspeedLogger.Debug( syncRunContext, CallerType,
				$"Using basic header authorization method for {this._config.Username}" );
			var authInfo = string.Concat( this._config.Username, ":", this._config.Password );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private static void LogRequestFailure( WebException ex, LightspeedRequest lightspeedRequest, HttpWebRequest request, SyncRunContext syncRunContext )
		{
			if( ex.Status != WebExceptionStatus.ProtocolError )
				return;
			if( !( ex.Response is HttpWebResponse response ) )
				return;

			var requestUri = request.Address.AbsolutePath;

			var lightspeedRequestBody = lightspeedRequest.ToJson();
			var safetyHeaders = TokenSanitizer.SanitizeBearerToken( request.Headers.ToString() );

			string responseText;
			using ( var reader = new StreamReader( ex.Response.GetResponseStream() ) )
			{
				responseText = reader.ReadToEnd();
			}

			LightspeedLogger.Error( syncRunContext, CallerType,
				$"Got {response.StatusCode} code response from server with message {ex.Message}. RequestUrl: {request.Method} {requestUri}, headers: {safetyHeaders}, body {lightspeedRequestBody}" );
		}

		private async Task< HttpWebResponse > GetWrappedAsyncResponse( Func< Task< WebResponse > > action, SyncRunContext syncRunContext, CancellationToken ct )
		{
			try
			{
				var response = await ActionPolicies.SubmitPolicyAsync( syncRunContext ).ExecuteAsync( () => this._throttler != null ? this._throttler.ExecuteAsync( action ) : action.Invoke() );
				ct.ThrowIfCancellationRequested();
				return ( HttpWebResponse )response;
			}
			catch( WebException ex )
			{
				if( ct.IsCancellationRequested )
					throw new OperationCanceledException( ex.Message, ex, ct );

				throw;
			}
		}
	}
}