using System;
using System.Collections.Generic;

namespace lightspeedAccess.Models.Request
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
				{ LightspeedRequestPathParam.TimeStamp, LightspeedDateRangeParamBuilder.GetDateDateRangeParam( this.FromDateUtc, this.ToDateUtc ) },
				{ LightspeedRequestPathParam.Limit, this.Limit.ToString() },
				{ LightspeedRequestPathParam.Offset, this.Offset.ToString() },
				{ LightspeedRequestPathParam.Completed, "true" },
				{ LightspeedRequestPathParam.LoadRelations, "[\"SaleLines\",\"Customer\",\"ShipTo\",\"Customer.Contact\",\"ShipTo.Contact\"]" }
			};
		}

		public GetSalesRequest( DateTime fromDateUtc, DateTime toDateUtc, int offset = 0, int limit = 50 )
		{
			this.Limit = limit;
			this.Offset = offset;
			this.FromDateUtc = fromDateUtc;
			this.ToDateUtc = toDateUtc;
		}

		public void SetOffset( int offset )
		{
			this.Offset = offset;
		}

		public int GetOffset()
		{
			return this.Offset;
		}

		public int GetLimit()
		{
			return this.Limit;
		}

		public override string ToString()
		{
			return "GetSalesRequest";
		}
	}
}