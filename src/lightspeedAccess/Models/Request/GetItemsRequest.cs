using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedAccess.Models.Request
{
	public class GetItemsRequest : LightspeedRequest
	{
		private readonly List<int> ItemIds;
		private readonly List<string> ItemSkus;
 
		protected override IEnumerable<LightspeedRestAPISegment> GetPath()
		{
			return new List<LightspeedRestAPISegment> { LightspeedRestAPISegment.Item };
		}

		protected override Dictionary<LightspeedRequestPathParam, string> GetPathParams()
		{
			if ( ItemIds != null )
			{
				if ( ItemIds.Count == 0 ) 
				return new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.ItemId, LightspeedIdRangeBuilder.GetIdRangeParam( ItemIds ) } };
			}

			if ( ItemSkus != null )
			{
				if ( ItemSkus.Count == 0 )
					return new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.SystemSku, LightspeedIdRangeBuilder.GetIdRangeParam( ItemSkus ) } };
			}
			return new Dictionary<LightspeedRequestPathParam, string>();
		}

		public GetItemsRequest(IEnumerable<int> ids )
		{
			ItemIds = ids.ToList();
		}

		public GetItemsRequest( IEnumerable<string> skus )
		{
			ItemSkus = skus.ToList();
		}

		public override string ToString()
		{
			return "GetItemsRequest";
		}

	}

	public class SucessApiResponse
	{

	}
}
