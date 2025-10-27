using SkuVault.Lightspeed.Access.Misc;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Models.Configuration;
using SkuVault.Lightspeed.Access.Services;
using SkuVault.Integrations.Core.Logging;

namespace SkuVault.Lightspeed.Access
{
	/// <summary>
	/// Abstract base class that provides shared infrastructure for all Lightspeed API integration services.
	/// </summary>
	public abstract class LightspeedBaseService
	{
		/// <summary>
		/// Handles execution of authenticated HTTP requests to the Lightspeed API.
		/// Provides retry logic and integrates throttling support.
		/// </summary>
		internal readonly WebRequestService _webRequestServices;

		/// <summary>
		/// Provides contextual information for the current synchronization or integration run.
		/// Used to track progress, timing, and correlation data across services.
		/// </summary>
		internal readonly SyncRunContext _syncRunContext;

		/// <summary>
		/// Logging interface used for structured output, error tracking, and diagnostic tracing.
		/// </summary>
		internal readonly IIntegrationLogger _logger;

		/// <summary>
		/// Manages OAuth authentication and token refresh operations with the Lightspeed API.
		/// Ensures that all requests are authorized with valid access tokens.
		/// </summary>
		internal readonly LightspeedAuthService _authService;

		/// <summary>
		/// Contains configuration data for the Lightspeed integration, including client credentials,
		/// account identifiers, and API connection settings.
		/// </summary>
		internal readonly LightspeedConfig _config;

		/// <summary>
		/// Initializes a new instance of the <see cref="LightspeedBaseService"/> class.
		/// </summary>
		/// <param name="config">Configuration containing Lightspeed credentials and account information.</param>
		/// <param name="syncRunContext">Context object representing the current synchronization or integration run.</param>
		/// <param name="logger">Logger used for recording integration events and diagnostic messages.</param>
		protected LightspeedBaseService(LightspeedConfig config, SyncRunContext syncRunContext, IIntegrationLogger logger)
		{
			_config = config;
			_syncRunContext = syncRunContext;
			_logger = logger;

			_authService = new LightspeedAuthService(
				config.ClientId,
				config.ClientSecret,
				syncRunContext,
				logger
			);

			_webRequestServices = new WebRequestService(
				config,
				new ThrottlerAsync(config.AccountId, syncRunContext, logger),
				_authService,
				logger
			);
		}
	}
}