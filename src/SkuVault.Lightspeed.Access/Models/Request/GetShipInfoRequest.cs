using System.Collections.Generic;
using System.Linq;

namespace SkuVault.Lightspeed.Access.Models.Request
{
	public class GetShipInfoRequest: LightspeedRequest
	{
		private readonly List< int > _shipToIds;

		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			return new List< LightspeedRestAPISegment > { LightspeedRestAPISegment.ShipTo };
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >
			{
				{ LightspeedRequestPathParam.LoadRelations, "[\"Contact\"]" },
				{ LightspeedRequestPathParam.ShipToId, LightspeedIdRangeBuilder.GetIdRangeParam( _shipToIds ) }
			};
		}

		public GetShipInfoRequest( IEnumerable< int > shipToIds )
		{
			_shipToIds = shipToIds.ToList();
		}

		public override string ToString()
		{
			return "GetShipInfoRequest";
		}
	}
}