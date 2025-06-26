using System.Net;
using lightspeedAccess.Models.Common;

namespace lightspeedAccess.Misc
{
	static class QuotaParser
	{
		public static bool TryParseQuota< T >( T body, out ResponseLeakyBucketMetadata metadata )
		{
			metadata = null;
			if( !( body is HttpWebResponse ) )
				return false;

			var rawResponse = body as HttpWebResponse;
			var bucketHeader = rawResponse.Headers[ "X-LS-API-Bucket-Level" ];
			if ( string.IsNullOrWhiteSpace( bucketHeader ) ) return false;
			var parsedQuotaInfo = bucketHeader.Split( '/' );
			if( parsedQuotaInfo.Length != 2 ) return false;
			int hypotheticQuotaSize;
			float hypotheticQuotaUsed;
			if( !float.TryParse( parsedQuotaInfo[ 0 ], out hypotheticQuotaUsed ) || !int.TryParse( parsedQuotaInfo[ 1 ], out hypotheticQuotaSize ) ) return false;

			metadata = new ResponseLeakyBucketMetadata
			{
				quotaSize = hypotheticQuotaSize,
				quotaUsed = ( int )hypotheticQuotaUsed,
				dripRate = 1
			};

			var dripRateHeader = rawResponse.Headers[ "X-LS-API-Drip-Rate" ];
			if( !string.IsNullOrWhiteSpace( dripRateHeader ) )
			{
				float dripRate;
				if( !float.TryParse( dripRateHeader, out dripRate ) )
					dripRate = 1;

				metadata.dripRate = dripRate > 0 ? dripRate : 1;
			}

			return true;
		}
	}
}