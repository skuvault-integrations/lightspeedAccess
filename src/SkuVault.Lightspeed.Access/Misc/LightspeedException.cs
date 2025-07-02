using System;

namespace SkuVault.Lightspeed.Access.Misc
{
	public class LightspeedException : Exception
	{
		public LightspeedException( string message, Exception exception )
			: base( "[Lightspeed] " + message, exception )
		{
		}

		public LightspeedException( string message )
			: base( "[Lightspeed] " + message )
		{
		}
	}
}