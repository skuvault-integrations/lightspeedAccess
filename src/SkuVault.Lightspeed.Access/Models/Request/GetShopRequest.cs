using System.Collections.Generic;

namespace SkuVault.Lightspeed.Access.Models.Request
{
	internal class GetShopRequest: LightspeedRequest, IRequestPagination
	{
		public int Limit{ get; private set; }
		public int Offset{ get; private set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Shop };
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >
			{
				{ LightspeedRequestPathParam.Limit, Limit.ToString() },
				{ LightspeedRequestPathParam.Offset, Offset.ToString() }
			};
		}

		public GetShopRequest()
		{
			Limit = 100;
			Offset = 0;
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
			return "GetShopRequest";
		}
	}
}