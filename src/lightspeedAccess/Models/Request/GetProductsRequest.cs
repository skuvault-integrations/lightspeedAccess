using System;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedAccess.Models.Request
{
	public class GetProductsRequest: LightspeedRequest, IRequestPagination
	{
		internal const int DefaultLimit = 50;

		private readonly int ShopId;

		private int Limit{ get; set; }
		private int Offset{ get; set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Item };
		}

		private Dictionary< LightspeedRequestPathParam, string > GetMainPathParams()
		{
			var pathParams = new Dictionary<LightspeedRequestPathParam, string>
			{ 
				{ LightspeedRequestPathParam.ShopId, this.ShopId.ToString() },
				{ LightspeedRequestPathParam.LoadRelations, "[\"Category\",\"ItemShops\",\"Images\",\"ItemAttributes\",\"ItemAttributes.ItemAttributeSet\"]" } 
			};
			return pathParams;
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			var initialParams = this.GetMainPathParams();
			initialParams.Add( LightspeedRequestPathParam.Limit, this.Limit.ToString() );
			initialParams.Add( LightspeedRequestPathParam.Offset, this.Offset.ToString() );

			return initialParams;
		}

		private void InitRequest()
		{
			this.Limit = DefaultLimit;
			this.Offset = 0;
		}

		public GetProductsRequest( int shopId )
		{
			this.InitRequest();
			this.ShopId = shopId;
		}

		public override string ToString()
		{
			return "GetProductsRequest";
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