using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;

namespace lightspeedAccess.Misc
{
	class LightspeedLogger
	{
		public static ILogger Log{ get; private set; }

		static LightspeedLogger()
		{
			Log = NetcoLogger.GetLogger( "LightspeedLogger" );
		}
	}
}
