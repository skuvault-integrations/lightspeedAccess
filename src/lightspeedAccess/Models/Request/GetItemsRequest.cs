using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Models.Request
{
	public class GetItemsRequest : LightspeedRequest
	{
		private readonly List<int> ItemIds = new List<int>();
 
		protected override IEnumerable<LightspeedRestAPISegment> GetPath()
		{
			return new List<LightspeedRestAPISegment> { LightspeedRestAPISegment.Item };
		}

		protected override Dictionary<LightspeedRequestPathParam, string> GetPathParams()
		{
			if (ItemIds.Count == 0) return new Dictionary<LightspeedRequestPathParam, string>();
			return new Dictionary<LightspeedRequestPathParam, string>
			{ { LightspeedRequestPathParam.ItemId, LightspeedIdRangeBuilder.GetIdRangeParam(ItemIds) } };
		}

		public GetItemsRequest(IEnumerable<int> ids )
		{
			ItemIds = ids.ToList();
		}
	}

	public class SucessApiResponse
	{

	}
}
