using System.Collections.Generic;

namespace SkuVault.Lightspeed.Access.Models.Request
{
	public class GetVendorsRequest: LightspeedRequest, IRequestPagination
	{
		internal const int DefaultLimit = 50;
		
		private readonly int ShopId;

		private int Limit{ get; set; }
		private int Offset{ get; set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Vendor };
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			var initialParams = new Dictionary< LightspeedRequestPathParam, string >
			{
				{ LightspeedRequestPathParam.Limit, this.Limit.ToString() },
				{ LightspeedRequestPathParam.Offset, this.Offset.ToString() }
			};

			return initialParams;
		}

		private void InitRequest()
		{
			this.Limit = DefaultLimit;
			this.Offset = 0;
		}

		public GetVendorsRequest( int shopId )
		{
			this.InitRequest();
			this.ShopId = shopId;
		}

		public override string ToString()
		{
			return "GetVendorsRequest";
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
	}
}