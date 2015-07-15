using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedAccess.Models.Request
{
	public class GetSalesRequest: LightspeedRequest, IRequestPagination
	{
		public int Limit{ get; private set; }
		public int Offset{ get; private set; }

		public DateTime FromDateUtc{ get; private set; }
		public DateTime ToDateUtc{ get; private set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment >
			{ LightspeedRestAPISegment.Sale };
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >
			{
				{ LightspeedRequestPathParam.TimeStamp, LightspeedDateRangeParamBuilder.GetDateDateRangeParam( FromDateUtc, ToDateUtc ) },
				{ LightspeedRequestPathParam.Limit, Limit.ToString() },
				{ LightspeedRequestPathParam.Offset, Offset.ToString() },
				{ LightspeedRequestPathParam.Completed, "true" },
				{ LightspeedRequestPathParam.LoadRelations, "[\"SaleLines\"]" }
			};
		}

		public GetSalesRequest( DateTime fromDateUtc, DateTime toDateUtc, int offset = 0, int limit = 100 )
		{
			Limit = limit;
			Offset = offset;
			FromDateUtc = fromDateUtc;
			ToDateUtc = toDateUtc;
		}

		public void SetOffset( int offset )
		{
			Offset = offset;
		}

		public int GetOffset()
		{
			return Offset;
		}

		public int GetLimit()
		{
			return Limit;
		}

		public override string ToString()
		{
			return "GetSalesRequest";
		}
	}
}