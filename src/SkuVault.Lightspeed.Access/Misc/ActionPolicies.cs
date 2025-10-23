using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SkuVault.Integrations.Core.Common;
using SkuVault.Integrations.Core.Logging;
using SkuVault.Lightspeed.Access.Services;

namespace SkuVault.Lightspeed.Access.Misc
{
	internal class ActionPolicies
	{
		private static readonly int RetryIntervalSeconds = 45;

		public static RetryPolicy SubmitPolicy( SyncRunContext syncRunContext, IIntegrationLogger logger )
		{
			return Policy
				.Handle< Exception >()
				.Retry( 10, ( ex, i ) =>
			{
				logger.Logger.LogWarning(
					Constants.LoggingCommonPrefix + "Retrying Lightspeed API submit call for the {IndexCall} time with exception {ExceptionMessage}",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext?.TenantId,
					syncRunContext?.ChannelAccountId,
					syncRunContext?.CorrelationId,
					nameof(ActionPolicies),
					nameof(SubmitPolicy),
					i,
					ex.Message );

				if ( !LightspeedAuthService.IsUnauthorizedException( ex ) && !WebRequestService.IsBadRequestException( ex ) )
				{
					Thread.Sleep( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
				}
			} );
		}

		public static AsyncRetryPolicy SubmitPolicyAsync( SyncRunContext syncRunContext, IIntegrationLogger logger )
		{
			return Policy
				.Handle< Exception >()
				.RetryAsync( 10, async ( ex, i ) =>
			{
				logger.Logger.LogWarning(
					Constants.LoggingCommonPrefix + "Retrying Lightspeed API submit call for the {IndexCall} time with exception {ExceptionMessage}",
					Constants.ChannelName,
					Constants.VersionInfo,
					syncRunContext?.TenantId,
					syncRunContext?.ChannelAccountId,
					syncRunContext?.CorrelationId,
					nameof(ActionPolicies),
					nameof(SubmitPolicyAsync),
					i,
					ex.Message );

				if ( !LightspeedAuthService.IsUnauthorizedException( ex ) && !WebRequestService.IsBadRequestException( ex ) )
				{
					await Task.Delay( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
				}
			} );
		}
	}
}