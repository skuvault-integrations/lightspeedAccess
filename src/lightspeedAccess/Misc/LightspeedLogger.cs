using System;
using System.Diagnostics;
using System.Reflection;
using Netco.Logging;
using SkuVault.Integrations.Core.Common;

namespace LightspeedAccess.Misc
{
	internal class LightspeedLogger
	{
		private static ILogger Log{ get; set; }
		private static readonly string VersionInfo;
		private const string IntegrationName = "Lightspeed";

		static LightspeedLogger()
		{
			Log = NetcoLogger.GetLogger( "LightspeedLogger" );
			var assembly = Assembly.GetExecutingAssembly();
			VersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
		}

		public static void Debug( SyncRunContext syncRunContext, string callerType, string callerMethodName, string message )
		{
			Log.Debug( FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}
			
		public static void Error( SyncRunContext syncRunContext, string callerType, string callerMethodName, string message )
		{
			Log.Error( FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}

		public static void Error( Exception ex, SyncRunContext syncRunContext, string callerType, string callerMethodName, string message )
		{
			Log.Error( ex, FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}

		private static string FormatMessage( SyncRunContext syncRunContext, string callerType, string callerMethodName, string message )
		{
			// LoggingCommonPrefix: [{Channel}] [{Version}] [{TenantId}] [{ChannelAccountId}] [{CorrelationId}] [{CallerType}] [{CallerMethodName}]: message
			return string.Format("[{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}]: {7}", IntegrationName, VersionInfo, syncRunContext.TenantId,
				syncRunContext.ChannelAccountId, syncRunContext.CorrelationId, callerType, callerMethodName, message );
		}
	}
}