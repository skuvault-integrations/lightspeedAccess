using System.Collections.Generic;

namespace SkuVault.Lightspeed.Access.Models.Request
{
	class GetItemRequest :LightspeedRequest
	{
		private readonly int itemId;

		public GetItemRequest( int itemId )
		{
			this.itemId = itemId;
		}

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment >
			{
				LightspeedRestAPISegment.Item, new LightspeedRestAPISegment( this.itemId )
			};
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >();
		}
	}
}
