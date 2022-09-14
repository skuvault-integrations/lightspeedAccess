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
		private ArchivedOptionEnum ArchivedOption{ get; set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Item };
		}

		private Dictionary< LightspeedRequestPathParam, string > GetMainPathParams()
		{
			if( this.ShopId != 0 )
			{
				var pathParams = new Dictionary<LightspeedRequestPathParam, string>
				{ 
					{ LightspeedRequestPathParam.ShopId, this.ShopId.ToString() },
					{ LightspeedRequestPathParam.LoadRelations, "[\"ItemShops\",\"Images\",\"ItemAttributes\",\"ItemAttributes.ItemAttributeSet\"]" } 
				};
				return pathParams;
			}

			return new Dictionary< LightspeedRequestPathParam, string >();
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			var initialParams = this.GetMainPathParams();
			initialParams.Add( LightspeedRequestPathParam.Limit, this.Limit.ToString() );
			initialParams.Add( LightspeedRequestPathParam.Offset, this.Offset.ToString() );
			if( this.ArchivedOption != ArchivedOptionEnum.Undefined )
			{
				initialParams.Add( LightspeedRequestPathParam.Archived, this.ArchivedOption.ToString().ToLowerInvariant() );
			}

			return initialParams;
		}

		private void InitRequest()
		{
			this.Limit = DefaultLimit;
			this.Offset = 0;
			this.ArchivedOption = ArchivedOptionEnum.Undefined;
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

		public void SetArchivedOptionEnum( ArchivedOptionEnum archived )
		{
			this.ArchivedOption = archived;
		}

		public enum ArchivedOptionEnum
		{
			Undefined,
			True, 
			False, 
			Only
		}
	}
}