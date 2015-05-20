using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightspeedAccess.Models.Request
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
	}
}