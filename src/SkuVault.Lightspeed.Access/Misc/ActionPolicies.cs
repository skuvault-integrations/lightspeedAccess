using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SkuVault.Integrations.Core.Common;
using SkuVault.Lightspeed.Access.Services;

namespace SkuVault.Lightspeed.Access.Misc
{
	internal class ActionPolicies
	{
		private static readonly int RetryIntervalSeconds = 45;

		public static RetryPolicy SubmitPolicy( SyncRunContext syncRunContext )
		{
			return Policy
				.Handle< Exception >()
				.Retry( 10, ( ex, i ) =>
			{
				LightspeedLogger.Error( ex, syncRunContext, nameof(ActionPolicies),
					$"Retrying Lightspeed API submit call for the {i} time" );
				
				if( !LightspeedAuthService.IsUnauthorizedException( ex ) && !WebRequestService.IsBadRequestException( ex ) )
				{
					Thread.Sleep( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
				}
			} );
		}

		public static AsyncRetryPolicy SubmitPolicyAsync( SyncRunContext syncRunContext )
		{
			return Policy
				.Handle< Exception >()
				.RetryAsync( 10, async ( ex, i ) =>
			{
				LightspeedLogger.Error( ex, syncRunContext, nameof(ActionPolicies),
					$"Retrying Lightspeed API submit call for the {i} time" );

				if( !LightspeedAuthService.IsUnauthorizedException( ex ) && !WebRequestService.IsBadRequestException( ex ) )
				{
					await Task.Delay( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
				}
			} );
		}
	}
}