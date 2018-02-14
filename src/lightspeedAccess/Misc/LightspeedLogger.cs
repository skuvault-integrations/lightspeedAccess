using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;

namespace LightspeedAccess.Misc
{
	internal class LightspeedLogger
	{
		private static ILogger Log{ get; set; }

		static LightspeedLogger()
		{
			Log = NetcoLogger.GetLogger( "LightspeedLogger" );
		}

		public static void Debug( string message, int accountId )
		{
			Log.Debug( FormatMessage( message, accountId ) );
		}

		public static void Error( string message, int accountId )
		{
			Log.Error( FormatMessage( message, accountId ) );
		}

		public static void Error( Exception ex, string message, int accountId )
		{
			Log.Error( ex, FormatMessage( message, accountId ) );
		}

		private static string FormatMessage( string message, int accountId )
		{
			return string.Format( "Account: {0}, {1}", accountId, message );
		}
	}
}