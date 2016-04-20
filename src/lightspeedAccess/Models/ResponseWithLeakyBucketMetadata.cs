using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace lightspeedAccess.Models
{
	class ResponseLeakyBucketMetadata
	{
		public int quotaSize;
		public int quotaUsed;
	}

	static class QuotaParser
	{
		public static bool TryParseQuota< T >( T body, out ResponseLeakyBucketMetadata metadata )
		{
			metadata = null;
			if( !( body is HttpResponse ) )
				return false;

			var rawResponse = body as HttpResponse;
			var bucketHeader = rawResponse.Headers[ "X-LS-API-Bucket-Level" ];
			if ( string.IsNullOrWhiteSpace( bucketHeader ) ) return false;
			var parsedQuotaInfo = bucketHeader.Split( '/' );
			if( parsedQuotaInfo.Length != 2 ) return false;
			int hypotheticQuotaSize;
			int hypotheticQuotaUsed;
			if( !int.TryParse( parsedQuotaInfo[ 0 ], out hypotheticQuotaUsed ) || !int.TryParse( parsedQuotaInfo[ 1 ], out hypotheticQuotaSize ) ) return false;

			metadata = new ResponseLeakyBucketMetadata
			{
				quotaSize = hypotheticQuotaSize,
				quotaUsed = hypotheticQuotaUsed
			};
			return true;
		}
	}
}
