using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedAccess.Models.Request
{
	public class GetItemsRequest: LightspeedRequest, IRequestPagination
	{
		private readonly List< int > ItemIds;
		private readonly List< string > ItemSkus;
		private readonly int ShopId;

		public int Limit { get; private set; }
		public int Offset { get; private set; }

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.Item };
		}

		private Dictionary<LightspeedRequestPathParam, string> GetMainPathParams()
		{
			if ( ItemIds != null )
			{
				if ( ItemIds.Count != 0 )
					return new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.ItemId, LightspeedIdRangeBuilder.GetIdRangeParam( ItemIds ) } };
			}

			if ( ItemSkus != null )
			{
				if ( ItemSkus.Count != 0 )
					return new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.Or, LightspeedSkuRangeBuilder.GetIdRangeParam( ItemSkus ) }, { LightspeedRequestPathParam.LoadRelations, "[\"ItemShops\"]" } };
			}

			if ( ShopId != 0 )
				return new Dictionary<LightspeedRequestPathParam, string> { { LightspeedRequestPathParam.ShopId, this.ShopId.ToString() }, { LightspeedRequestPathParam.LoadRelations, "[\"ItemShops\"]" } };

			return new Dictionary<LightspeedRequestPathParam, string>();
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			var initialParams = this.GetMainPathParams();
			initialParams.Add( LightspeedRequestPathParam.Limit, this.Limit.ToString() );
			initialParams.Add( LightspeedRequestPathParam.Offset, this.Offset.ToString() );

			return initialParams;
		}

		private void InitPagination()
		{
			Limit = 100;
			Offset = 0;
		}

		public GetItemsRequest( IEnumerable< int > ids )
		{
			this.InitPagination();
			ItemIds = ids.ToList();
		}

		public GetItemsRequest( IEnumerable< string > skus )
		{
			this.InitPagination();
			ItemSkus = skus.ToList();
		}

		public GetItemsRequest( int shopId )
		{
			this.InitPagination();
			ShopId = shopId;
		}

		public override string ToString()
		{
			return "GetItemsRequest";
		}

		public void SetOffset( int offset )
		{
			Offset = offset;
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

	public class SucessApiResponse
	{
	}
}