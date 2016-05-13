using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;

namespace LightspeedAccess.Misc
{
	internal class LightspeedLogger
	{
		public static ILogger Log{ get; private set; }

		static LightspeedLogger()
		{
			Log = new ConsoleLogger();  //NetcoLogger.GetLogger( "LightspeedLogger" );
		}

		private class ConsoleLogger : ILogger 
		{
			public void Trace( string message )
			{
			}

			public void Trace( Exception exception, string message )
			{
			}

			public void Trace( string format, params object[] args )
			{
			}

			public void Trace( Exception exception, string format, params object[] args )
			{
			}

			public void Debug( string message )
			{
			}

			public void Debug( Exception exception, string message )
			{
			}

			public void Debug( string format, params object[] args )
			{
			}

			public void Debug( Exception exception, string format, params object[] args )
			{
			}

			public void Info( string message )
			{
			}

			public void Info( Exception exception, string message )
			{
			}

			public void Info( string format, params object[] args )
			{
			}

			public void Info( Exception exception, string format, params object[] args )
			{
			}

			public void Warn( string message )
			{
				Console.WriteLine( message );
			}

			public void Warn( Exception exception, string message )
			{
			}

			public void Warn( string format, params object[] args )
			{
				Console.WriteLine( format, args );
			}

			public void Warn( Exception exception, string format, params object[] args )
			{
			}

			public void Error( string message )
			{
			}

			public void Error( Exception exception, string message )
			{
			}

			public void Error( string format, params object[] args )
			{
			}

			public void Error( Exception exception, string format, params object[] args )
			{
			}

			public void Fatal( string message )
			{
			}

			public void Fatal( Exception exception, string message )
			{
			}

			public void Fatal( string format, params object[] args )
			{
			}

			public void Fatal( Exception exception, string format, params object[] args )
			{
			}
		}
	}
}