using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LightspeedAccess.Models.Order;
using LightspeedAccess.Misc;
using LightspeedAccess.Models.Configuration;
using LightspeedAccess.Models.Request;

namespace LightspeedAccess.Services
{
	internal class WebRequestService
	{
		private readonly LightspeedConfig _config;
		private readonly ThrottlerAsync _throttler;

		public WebRequestService( LightspeedConfig config, ThrottlerAsync throttler )
		{
			this._config = config;
			this._throttler = throttler;
		}

		private static void ManageRequestBody( LightspeedRequest request, ref HttpWebRequest webRequest )
		{
			var body = request.GetBody();

			if( body == null )
				return;
			LightspeedLogger.Log.Debug( "Creating request body from stream for request {0}", request.ToString() );

			webRequest.Method = "PUT";

			webRequest.ContentType = "text/xml";

			body.Seek( 0, SeekOrigin.Begin );

			using ( var sr = new StreamReader( body ) )
			{
				var s = sr.ReadToEnd();

				LightspeedLogger.Log.Debug( "Created request body for request {0} : {1}", request.ToString(), s );

				webRequest.ContentLength = s.Length;
				Stream dataStream = webRequest.GetRequestStream();
				var bytes = Encoding.UTF8.GetBytes( s );
				foreach ( var singleByte in bytes )
				{
					dataStream.WriteByte( singleByte );
				}

				dataStream.Close();
			}
		}

		public T GetResponse< T >( LightspeedRequest request )
		{
			LightspeedLogger.Log.Debug( "Making request {0} to lightspeed server", request.ToString() );

			var webRequest = this.CreateHttpWebRequest( this._config.Endpoint + request.GetUri( this._config.LightspeedAuthToken ) );
			ManageRequestBody( request, ref webRequest );

			using ( var response = webRequest.GetResponse() )
			{
				var stream = response.GetResponseStream();

				LightspeedLogger.Log.Debug( "Got response from server for request {0}, starting deserialization", request.ToString() );
				var deserializer =
					new XmlSerializer( typeof( T ) );

				var result = ( T ) deserializer.Deserialize( stream );
				LightspeedLogger.Log.Debug( "Successfylly deserialized response for request {0}", request.ToString() );

				var possibleAdditionalResponses = this.IterateThroughPagination( request, result );

				var aggregatedResult = result as IPaginatedResponse;
				if ( aggregatedResult != null )
				{
					LightspeedLogger.Log.Debug( "Aggregating paginated results for request {0}", request.ToString() );
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		public async Task< T > GetResponseAsync< T >( LightspeedRequest request, CancellationToken ctx )
		{
			LightspeedLogger.Log.Debug( "Making request {0} to lightspeed server", request.ToString() );
			var webRequest = this.CreateHttpWebRequest( this._config.Endpoint + request.GetUri( this._config.LightspeedAuthToken ) );
			ManageRequestBody( request, ref webRequest );


			using ( var response = await ( this.GetWrappedAsyncResponse( webRequest, ctx ) ) )
			{
				var stream = response.GetResponseStream();

				LightspeedLogger.Log.Debug( "Got response from server for request {0}, starting deserialization", request.ToString() );
				var deserializer =
					new XmlSerializer( typeof( T ) );

				var result = ( T ) deserializer.Deserialize( stream );

				LightspeedLogger.Log.Debug( "Successfylly deserialized response for request {0}", request.ToString() );
				var possibleAdditionalResponses = await this.IterateThroughPaginationAsync( request, result, ctx );

				var aggregatedResult = result as IPaginatedResponse;

				if ( aggregatedResult != null )
				{
					LightspeedLogger.Log.Debug( "Aggregating paginated results for request {0}", request.ToString() );
					possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse ) resp ) );
				}

				response.Close();
				return result;
			}
		}

		private static bool NeedToIterateThroughPagination< T >( T response, LightspeedRequest r )
		{
			var paginatedResponse = response as IPaginatedResponse;
			var requestWithPagination = r as IRequestPagination;
			return ( paginatedResponse != null && requestWithPagination != null &&
			         paginatedResponse.GetCount() > requestWithPagination.GetLimit() );
		}

		private List< T > IterateThroughPagination< T >( LightspeedRequest r, T response )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, r ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )r;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			LightspeedLogger.Log.Debug( "Response for request {0} was paginated, started iterating the remaining pages...", r.ToString() );

			var numPages = paginatedResponse.GetCount() / paginatedRequest.GetLimit() + 1;

			LightspeedLogger.Log.Debug( "Expected number of pages for request {0} : {2}", r.ToString(), numPages );
			for( int pageNum = 1; pageNum < numPages; pageNum++ )
			{
				LightspeedLogger.Log.Debug( "Processing page {0} for request {1}...", numPages, r.ToString() );
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( this.GetResponse< T >( r ) );
			}

			return additionalResponses;
		}

		private async Task< List< T > > IterateThroughPaginationAsync< T >( LightspeedRequest r, T response, CancellationToken ctx )
		{
			var additionalResponses = new List< T >();

			if( !NeedToIterateThroughPagination( response, r ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination )r;
			var paginatedResponse = ( IPaginatedResponse )response;

			if( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			var numPages = paginatedResponse.GetCount() / paginatedRequest.GetLimit() + 1;

			LightspeedLogger.Log.Debug( "Expected number of pages for request {0} : {2}", r.ToString(), numPages );
			for( int pageNum = 1; pageNum < numPages; pageNum++ )
			{
				LightspeedLogger.Log.Debug( "Processing page {0} / {1} for request {2}...", pageNum, numPages, r.ToString() );
				paginatedRequest.SetOffset( pageNum * paginatedRequest.GetLimit() );
				additionalResponses.Add( await this.GetResponseAsync< T >( r, ctx ) );
			}

			return additionalResponses;
		}

		private HttpWebRequest CreateHttpWebRequest( string url )
		{
			LightspeedLogger.Log.Debug( "Composed lightspeed request URL: {0}", url );
			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );

			request.Method = WebRequestMethods.Http.Get;
			if( this._config.LightspeedAuthToken == null )
				request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
			request.Timeout = this._config.TimeoutSeconds * 1000;

			return request;
		}

		private string CreateAuthenticationHeader()
		{
			LightspeedLogger.Log.Debug( "Usign basic header authorization method {0} : {1}", this._config.Username, this._config.Password );
			var authInfo = string.Concat( this._config.Username, ":", this._config.Password );
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private static void LogRequestFailure( WebException ex, HttpWebRequest request )
		{
			if ( ex.Status == WebExceptionStatus.ProtocolError )
			{
				var response = ex.Response as HttpWebResponse;
				if ( response != null )
				{
					var requestUri = request.Address.AbsolutePath;
					var requestBody = "N/A";
					try
					{
						var requestBodyStream = request.GetRequestStream();
						requestBodyStream.Seek( 0, SeekOrigin.Begin );
						var sr = new StreamReader( requestBodyStream );
						requestBody = sr.ReadToEnd();
					}
					catch ( Exception )
					{

					}

					LightspeedLogger.Log.Debug( "Got {0} code response from server with message {1}. Request was: {2} with body {3}", response.StatusCode, ex.Message, requestUri, requestBody );
				}
			}
		}

		private async Task< HttpWebResponse > GetWrappedAsyncResponse( HttpWebRequest request, CancellationToken ct )
		{
			using( ct.Register( request.Abort ) )
			{
				try
				{
					var response = await this._throttler.ExecuteAsync( request.GetResponseAsync );
					ct.ThrowIfCancellationRequested();
					return ( HttpWebResponse )response;
				}
				catch( WebException ex )
				{
					LogRequestFailure( ex, request );
					if( ct.IsCancellationRequested )
						throw new OperationCanceledException( ex.Message, ex, ct );

					throw;
				}
			}
		}
	}
}