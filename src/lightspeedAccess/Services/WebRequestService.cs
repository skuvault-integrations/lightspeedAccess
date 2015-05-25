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
using lightspeedAccess.Models.Configuration;
using System.Xml;
using System.Xml.Serialization;
using lightspeedAccess.Misc;
using lightspeedAccess.Models.Order;
using lightspeedAccess.Models.Request;

namespace lightspeedAccess.Services
{
	internal class WebRequestService
	{
		private readonly LightspeedConfig _config;

		public WebRequestService( LightspeedConfig config )
		{
			_config = config;
		}

		private static void ManageRequestBody( LightspeedRequest request, ref HttpWebRequest webRequest )
		{
			var body = request.GetBody();
			
			if( body == null )
				return;

			webRequest.Method = "PUT";

			webRequest.ContentType = "text/xml";

			body.Seek( 0, SeekOrigin.Begin );
			var sr = new StreamReader( body );
			var s = sr.ReadToEnd();

			webRequest.ContentLength = s.Length;
			Stream dataStream = webRequest.GetRequestStream();
			
			for ( var i = 0; i < s.Length; i++ )
			{
				dataStream.WriteByte( Convert.ToByte( s[ i ] ) );
			}
			dataStream.Close();
		}

		public T GetResponse< T >( LightspeedRequest request )
		{
			var webRequest = this.CreateHttpWebRequest( _config.Endpoint + request.GetUri() );
			ManageRequestBody( request, ref webRequest );

			var response = webRequest.GetResponse();
			var stream = response.GetResponseStream();

			var deserializer =
				new XmlSerializer( typeof( T ) );

			var result = ( T )deserializer.Deserialize( stream );

			var possibleAdditionalResponses = IterateThroughPagination< T >( request, result );

			var aggregatedResult = result as IPaginatedResponse;
			if( aggregatedResult != null )
				possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse )resp ) );

			return result;
		}

		public async Task<T> GetResponseAsync<T>( LightspeedRequest request, CancellationToken ctx )
		{
			var webRequest = this.CreateHttpWebRequest( _config.Endpoint + request.GetUri() );
			ManageRequestBody( request, ref webRequest );
			
			var response = await (GetWrappedAsyncResponse(webRequest, ctx));
			var stream = response.GetResponseStream();

			var deserializer =
				new XmlSerializer( typeof( T ) );

			var result = ( T )deserializer.Deserialize( stream );

			var possibleAdditionalResponses = await IterateThroughPaginationAsync< T >( request, result, ctx );

			var aggregatedResult = result as IPaginatedResponse;
			if( aggregatedResult != null )
				possibleAdditionalResponses.ForEach( resp => aggregatedResult.Aggregate( ( IPaginatedResponse )resp ) );

			return result;
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

			var numPages = paginatedRequest.GetLimit() / paginatedResponse.GetCount() + 1;

			for( int pageNum = 1; pageNum < numPages; pageNum++ )
			{
				paginatedRequest.SetOffset( pageNum * 10 );
				additionalResponses.Add( GetResponse< T >( r ) );
			}

			return additionalResponses;
		}

		private async Task<List<T>> IterateThroughPaginationAsync<T>( LightspeedRequest r, T response, CancellationToken ctx )
		{
			var additionalResponses = new List<T>();

			if ( !NeedToIterateThroughPagination( response, r ) )
				return additionalResponses;

			var paginatedRequest = ( IRequestPagination ) r;
			var paginatedResponse = ( IPaginatedResponse ) response;

			if ( paginatedRequest.GetOffset() != 0 )
				return additionalResponses;

			var numPages = paginatedRequest.GetLimit() / paginatedResponse.GetCount() + 1;

			for ( int pageNum = 1; pageNum < numPages; pageNum++ )
			{
				paginatedRequest.SetOffset( pageNum * 10 );
			    additionalResponses.Add( await GetResponseAsync<T>( r, ctx ) );
			}

			return additionalResponses;
		}

		private HttpWebRequest CreateHttpWebRequest( string url )
		{
			LightspeedLogger.Log.Debug( "Composed lightspeed request URL: {0}", url );
			var uri = new Uri( url );
			var request = ( HttpWebRequest )WebRequest.Create( uri );
			
			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( "Authorization", this.CreateAuthenticationHeader() );
			request.Timeout = _config.TimeoutSeconds * 1000;

			return request;
		}


		private string CreateAuthenticationHeader()
		{
			var authInfo = this._config.ApiKey == null ? string.Concat( this._config.Username, ":", this._config.Password ) : string.Concat( this._config.ApiKey, ":", "apikey" );  
			authInfo = Convert.ToBase64String( Encoding.Default.GetBytes( authInfo ) );

			return string.Concat( "Basic ", authInfo );
		}

		private static async Task<HttpWebResponse> GetWrappedAsyncResponse( HttpWebRequest request, CancellationToken ct )
		{
			using ( ct.Register( request.Abort ) )
			{
				try
				{
					var response = await request.GetResponseAsync();
					ct.ThrowIfCancellationRequested();
					return ( HttpWebResponse ) response;
				}
				catch ( WebException ex )
				{
					if ( ct.IsCancellationRequested )
					{
						throw new OperationCanceledException( ex.Message, ex, ct );
					}

					throw;
				}
			}
		}
	}
}