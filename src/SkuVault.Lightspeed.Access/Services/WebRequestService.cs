using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SkuVault.Lightspeed.Access.Helpers;
using SkuVault.Lightspeed.Access.Misc;
using SkuVault.Lightspeed.Access.Models.Request;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Integrations.Core.Logging;
using Microsoft.Extensions.Logging;

namespace SkuVault.Lightspeed.Access.Services
{
	internal class WebRequestService
	{
		private readonly LightspeedConfig _config;
		private readonly ThrottlerAsync _throttler;
		private readonly ILigthspeedAuthService _authService;
		private const string CallerType = nameof(WebRequestService);
		private readonly IIntegrationLogger _logger;

		public WebRequestService( LightspeedConfig config, ThrottlerAsync throttler, ILigthspeedAuthService authService, IIntegrationLogger logger )
		{
			_config = config;
			_throttler = throttler;
			_authService = authService;
			_logger = logger;
		}

		private void ManageRequestBody( LightspeedRequest request, ref HttpWebRequest webRequest, SyncRunContext syncRunContext )
		{
			var body = request.GetBody();

			if( body == null )
				return;

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "[Start]: Creating request body from stream for request: '{Request}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				syncRunContext.TenantId,
				syncRunContext.ChannelAccountId,
				syncRunContext.CorrelationId,
				CallerType,
				nameof(ManageRequestBody),
				request );

			webRequest.Method = "PUT";

			webRequest.ContentType = "text/xml";

			body.Seek( 0, SeekOrigin.Begin );

			using ( var sr = new StreamReader( body ) )
			{
				var requestBody = sr.ReadToEnd();

				webRequest.ContentLength = requestBody.Length;
				using( var dataStream = webRequest.GetRequestStream() )
				{
					var bytes = Encoding.UTF8.GetBytes( requestBody );
					foreach ( var singleByte in bytes )
					{
						dataStream.WriteByte( singleByte );
					}
				}

				_logger.Logger.LogInformation(
					Constants.LoggingCommonPrefix + "[End]: Created request body: '{RequestBody}'",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext.TenantId,
					syncRunContext.ChannelAccountId,
					syncRunContext.CorrelationId,
					CallerType,
					nameof(ManageRequestBody),
					requestBody );
			}
		}

		public T GetResponse< T >( LightspeedRequest request, SyncRunContext syncRunContext )
		{
			_logger.LogOperationStart( syncRunContext, CallerType );

			var webRequestAction = new Func< WebResponse >(() =>
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
							this.RefreshSession();
						}
						throw;
					}
				}
			);

			using( var response = ActionPolicies.SubmitPolicy( syncRunContext, _logger ).Execute( () => webRequestAction() ) )
			{
				var stream = response.GetResponseStream();

				using var reader = new StreamReader( stream, Encoding.UTF8 );
				var rawResponseXml = reader.ReadToEnd();

				_logger.Logger.LogInformation(
					Constants.LoggingCommonPrefix + "Raw response: '{RawResponse}'",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext.TenantId,
					syncRunContext.ChannelAccountId,
					syncRunContext.CorrelationId,
					CallerType,
					nameof(GetResponse),
					rawResponseXml );

				var deserializer = new XmlSerializer( typeof( T ) );
				using var stringReader = new StringReader( rawResponseXml );
				var result = ( T ) deserializer.Deserialize( stringReader );

				var possibleAdditionalResponses = this.IterateThroughPagination( request, result, syncRunContext );

				if( result is IPaginatedResponse aggregatedResult )
				{
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		public async Task< T > GetResponseAsync< T >( LightspeedRequest request, SyncRunContext syncRunContext, CancellationToken ctx )
		{
			_logger.LogOperationStart( syncRunContext, CallerType );

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
							this.RefreshSession();
						}
						if( IsItemNotFound( request, ex ) || IsHttp422Error( request, ex ) )
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

			using ( var response = await this.GetWrappedAsyncResponse( webRequestAction, syncRunContext, ctx ) )
			{
				if( response == null )
					return default;

				var stream = response.GetResponseStream();
				var deserializer =
					new XmlSerializer( typeof( T ) );

				var result = ( T ) deserializer.Deserialize( stream );

				_logger.Logger.LogInformation(
					Constants.LoggingCommonPrefix + "[End]: Successfully deserialized response: '{ResultJson}'",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext.TenantId,
					syncRunContext.ChannelAccountId,
					syncRunContext.CorrelationId,
					CallerType,
					nameof(GetResponseAsync),
					result.ToJson() );

				var possibleAdditionalResponses = await this.IterateThroughPaginationAsync( request, result, syncRunContext, ctx );

				if ( result is IPaginatedResponse aggregatedResult )
				{
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		private void RefreshSession()
		{
			this._config.LightspeedAccessToken = this._authService.GetNewAccessToken( this._config.LightspeedRefreshToken );
		}

		private static bool IsItemNotFound( LightspeedRequest request, WebException ex )
		{
			if( !( request is GetItemRequest || request is UpdateOnHandQuantityRequest ) || ex.Status != WebExceptionStatus.ProtocolError )
			{
				return false;
			}

			if ( ex.Response is not HttpWebResponse response )
				return false;

			return response.StatusCode == HttpStatusCode.NotFound;
		}

		private static bool IsHttp422Error( LightspeedRequest request, WebException exception )
		{
			// only UpdateOnHandQuantityRequest can trigger this error.
			if ( request is not UpdateOnHandQuantityRequest )
				return false;

			// 422 only comes as an HTTP protocol error
			if ( exception.Status != WebExceptionStatus.ProtocolError )
				return false;

			// we need a valid HTTP response to inspect the status code.
			if ( exception.Response is not HttpWebResponse response )
				return false;

			// Lightspeed may return HTTP 422 when updating the on-hand quantity for items
			// that have duplicated EAN/UPC values. In this case we skip the item instead
			// of failing the entire sync.
			return ( int )response.StatusCode == 422;
		}

		internal static bool IsBadRequestException( Exception exception )
		{
			if ( exception is not WebException webException )
				return false;

			if ( webException.Response is not HttpWebResponse response )
				return false;

			return response.StatusCode == HttpStatusCode.BadRequest;
		}

		private static bool NeedToIterateThroughPagination< T >( T response, LightspeedRequest r )
		{
			var paginatedResponse = response as IPaginatedResponse;
			var requestWithPagination = r as IRequestPagination;
			return paginatedResponse != null && requestWithPagination != null &&
				paginatedResponse.GetCount() > requestWithPagination.GetLimit();
		}

		private List< T > IterateThroughPagination< T >( LightspeedRequest request, T response, SyncRunContext syncRunContext )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, request ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )request;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			var numPages = ( paginatedResponse.GetCount() / paginatedRequest.GetLimit() ) + 1;

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "Expected number of pages for request '{Request}' : '{NumPages}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				syncRunContext.TenantId,
				syncRunContext.ChannelAccountId,
				syncRunContext.CorrelationId,
				CallerType,
				nameof(IterateThroughPagination),
				request,
				numPages );

			for( var pageNum = 1; pageNum < numPages; pageNum++ )
			{
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( this.GetResponse< T >( request, syncRunContext ) );
			}

			return additionalResponses;
		}

		private async Task< List< T > > IterateThroughPaginationAsync< T >( LightspeedRequest request, T response, SyncRunContext syncRunContext, CancellationToken ctx )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, request ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )request;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			var numPages = ( paginatedResponse.GetCount() / paginatedRequest.GetLimit() ) + 1;

			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "Expected number of pages for request '{Request}' : '{NumPages}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				syncRunContext.TenantId,
				syncRunContext.ChannelAccountId,
				syncRunContext.CorrelationId,
				CallerType,
				nameof(IterateThroughPaginationAsync),
				request,
				numPages );

			for( var pageNum = 1; pageNum < numPages; pageNum++ )
			{
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( await this.GetResponseAsync< T >( request, syncRunContext, ctx ) );
			}

			return additionalResponses;
		}

		private HttpWebRequest CreateHttpWebRequest( string url, SyncRunContext syncRunContext )
		{
			_logger.Logger.LogInformation(
				Constants.LoggingCommonPrefix + "Composed lightspeed request URL: '{url}'",
				Constants.ChannelName,
				Constants.VersionInfo,
				syncRunContext.TenantId,
				syncRunContext.ChannelAccountId,
				syncRunContext.CorrelationId,
				CallerType,
				nameof(CreateHttpWebRequest),
				url );

			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			request.Headers[ HttpRequestHeader.Authorization ] = this.CreateAuthenticationHeader();
			request.Timeout = this._config.TimeoutSeconds * 1000;

			return request;
		}

		private string CreateAuthenticationHeader()
		{
			if( !string.IsNullOrWhiteSpace( this._config.LightspeedAccessToken ) )
			{
				return string.Concat( "Bearer ", this._config.LightspeedAccessToken );
			}

			var authInfo = string.Concat( this._config.Username, ":", this._config.Password );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private void LogRequestFailure( WebException ex, LightspeedRequest lightspeedRequest, HttpWebRequest request, SyncRunContext syncRunContext )
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

			_logger.Logger.LogWarning(
					Constants.LoggingCommonPrefix + "Got '{StatusCode}' code response from server with message '{ErrorMessage}'. " +
						"RequestUrl: '{RequestMethod}' '{RequestUri}', headers: '{SafetyHeaders}', body '{LightspeedRequestBody}'",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext?.TenantId,
					syncRunContext?.ChannelAccountId,
					syncRunContext?.CorrelationId,
					CallerType,
					nameof(LogRequestFailure),
					response.StatusCode,
					ex.Message,
					request.Method,
					safetyHeaders,
					lightspeedRequestBody );
		}

		private async Task< HttpWebResponse > GetWrappedAsyncResponse( Func< Task< WebResponse > > action, SyncRunContext syncRunContext, CancellationToken ct )
		{
			try
			{
				var response = await ActionPolicies.SubmitPolicyAsync( syncRunContext, _logger ).ExecuteAsync( () =>
					this._throttler != null
						? this._throttler.ExecuteAsync( action )
						: action.Invoke() );

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