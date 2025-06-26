using System;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedAccess.Models.Request
{
	public class GetItemsRequest: LightspeedRequest, IRequestPagination
	{
		internal const int DefaultLimit = 50;

		private readonly List< int > ItemIds;
		private readonly List< string > ItemSkus;
		private readonly int ShopId;
		private readonly DateTime? createTimeUtc;

		private int Limit{ get; set; }
		private int Offset{ get; set; }
		private ArchivedOptionEnum ArchivedOption{ get; set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Item };
		}

		private Dictionary< LightspeedRequestPathParam, string > GetMainPathParams()
		{
			if( this.ItemIds != null )
			{
				if( this.ItemIds.Count != 0 )
					return new Dictionary< LightspeedRequestPathParam, string > { { LightspeedRequestPathParam.ItemId, LightspeedIdRangeBuilder.GetIdRangeParam( this.ItemIds ) } };
			}

			if( this.ItemSkus != null )
			{
				if( this.ItemSkus.Count != 0 )
					return new Dictionary< LightspeedRequestPathParam, string > { { LightspeedRequestPathParam.Or, LightspeedSkuRangeBuilder.GetIdRangeParam( this.ItemSkus ) }, { LightspeedRequestPathParam.LoadRelations, "[\"ItemShops\"]" } };
			}

			if( this.ShopId != 0 )
			{
				var pathParams = new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.ShopId, this.ShopId.ToString() }, { LightspeedRequestPathParam.LoadRelations, "[\"ItemShops\"]" } };
				if( this.createTimeUtc != null )
					pathParams.Add( LightspeedRequestPathParam.TimeStamp, LightspeedGreaterThanBuilder.GetDateGreaterParam( this.createTimeUtc.Value ) );
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

		public GetItemsRequest( IEnumerable< int > ids )
		{
			this.InitRequest();
			this.ItemIds = ids.ToList();
		}

		public GetItemsRequest( IEnumerable< string > skus )
		{
			this.InitRequest();
			this.ItemSkus = skus.ToList();
		}

		public GetItemsRequest( int shopId )
		{
			this.InitRequest();
			this.ShopId = shopId;
		}

		public GetItemsRequest( int shopId, DateTime createTimeUtc )
		{
			this.InitRequest();
			this.ShopId = shopId;
			this.createTimeUtc = createTimeUtc;
		}

		public override string ToString()
		{
			return "GetItemsRequest";
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