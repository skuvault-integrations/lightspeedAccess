using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SkuVault.Integrations.Core.Common;

namespace SkuVault.Lightspeed.Access.Misc
{
	internal static class LightspeedLogger
	{
		private static readonly ILogger<SyncRunContext> Log;
		private static readonly string VersionInfo;
		private const string IntegrationName = "Lightspeed";

		static LightspeedLogger()
		{
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.SetMinimumLevel( LogLevel.Information );
			});

			Log = loggerFactory.CreateLogger< SyncRunContext >();

			// Версионная информация
			var assembly = Assembly.GetExecutingAssembly();
			VersionInfo = FileVersionInfo.GetVersionInfo( assembly.Location ).FileVersion;
		}

		public static void Info( SyncRunContext syncRunContext, string callerType, string message,
			[CallerMemberName] string callerMethodName = null )
		{
			Log.LogInformation( FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}

		public static void Error( SyncRunContext syncRunContext, string callerType, string message,
			[CallerMemberName] string callerMethodName = null )
		{
			Log.LogError( FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}

		public static void Error( Exception ex, SyncRunContext syncRunContext, string callerType, string message,
			[CallerMemberName] string callerMethodName = null )
		{
			Log.LogError( ex,FormatMessage( syncRunContext, callerType, callerMethodName, message ) );
		}

		private static string FormatMessage( SyncRunContext ctxt, string callerType, string callerMethodName, string message )
		{
			return string.Format(
				"[{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}]: {7}",
				IntegrationName,
				VersionInfo,
				ctxt.TenantId,
				ctxt.ChannelAccountId,
				ctxt.CorrelationId,
				callerType,
				callerMethodName,
				message );
		}
	}
}