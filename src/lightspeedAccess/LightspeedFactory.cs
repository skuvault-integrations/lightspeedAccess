using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lightspeedAccess.Models.Configuration;

namespace lightspeedAccess
{
	interface ILightspeedFactory
	{
		ILightspeedOrdersService CreateOrdersService( LightspeedConfig config );
	}

	public sealed class LightspeedFactory : ILightspeedFactory
	{
		public ILightspeedOrdersService CreateOrdersService(LightspeedConfig config)
		{
				return new LightspeedOrdersService(config);
		}
	}
}
