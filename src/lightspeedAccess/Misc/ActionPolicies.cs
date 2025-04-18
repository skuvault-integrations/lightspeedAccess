using System;
using System.Threading.Tasks;
using lightspeedAccess;
using LightspeedAccess.Services;
using Netco.ActionPolicyServices;
using Netco.Utils;
using SkuVault.Integrations.Core.Common;

namespace LightspeedAccess.Misc
{
	internal class ActionPolicies
	{
		private static readonly int RetryIntervalSeconds = 45;

		public static ActionPolicy SubmitPolicy( SyncRunContext syncRunContext )
		{
			return ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				LightspeedLogger.Error( ex, syncRunContext, nameof(ActionPolicies),
					$"Retrying Lightspeed API submit call for the {i} time" );
				
				if( !LightspeedAuthService.IsUnauthorizedException( ex ) && !WebRequestService.IsBadRequestException( ex ) )
				{
					SystemUtil.Sleep( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
				}
			} );
		}

		public static ActionPolicyAsync SubmitPolicyAsync( SyncRunContext syncRunContext )
		{
			return ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
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