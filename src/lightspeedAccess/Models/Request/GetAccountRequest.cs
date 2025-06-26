using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightspeedAccess.Models.Request;

namespace lightspeedAccess.Models.Request
{
	internal class GetAccountRequest: LightspeedRequest
	{
		protected override IEnumerable< LightspeedRestAPISegment > GetPath()
		{
			yield break;
		}

		protected override Dictionary< LightspeedRequestPathParam, string > GetPathParams()
		{
			return new Dictionary< LightspeedRequestPathParam, string >();
		}

		public override string ToString()
		{
			return "GetAccountRequest";
		}
	}
}