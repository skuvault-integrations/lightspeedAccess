using Microsoft.Extensions.Logging;
using SkuVault.Integrations.Core.Logging;

namespace SkuVault.Lightspeed.Access.Misc
{
	public class LightspeedLogger : IIntegrationLogger
	{
		private readonly ILogger< LightspeedLogger > _logger;

		public LightspeedLogger( ILogger< LightspeedLogger > logger, LoggingContext loggingContext )
		{
			_logger = logger;
			LoggingContext = loggingContext;
		}
		public ILogger Logger => _logger;
		public LoggingContext LoggingContext { get; }
	}
}