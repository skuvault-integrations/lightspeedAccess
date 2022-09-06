using System;
using System.Collections.Generic;
using LightspeedAccess.Models.Request;

namespace lightspeedAccess.Models.Request
{
	public class GetProductRequest : LightspeedRequest
	{
		private readonly int _productId;

		public GetProductRequest( int productId )
		{
			this._productId = productId;
		}

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment >
			{
				LightspeedRestAPISegment.Item, new LightspeedRestAPISegment( this._productId )
			};
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >();
		}
	}
}
