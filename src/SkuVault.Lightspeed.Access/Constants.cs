using System.Diagnostics;
using System.Reflection;

namespace SkuVault.Lightspeed.Access
{
	/// <summary>
	/// Project-wise constants.
	/// </summary>
	internal static class Constants
	{
		public const string LoggingCommonPrefix = "[{Channel}] [{Version}] [{TenantId}] [{ChannelAccountId}] [{CorrelationId}] [{CallerType}] [{CallerMethodName}] ";
		public static readonly string VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
		public const string ChannelName = "lightspeed";
	}
}