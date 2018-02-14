using System;
using System.Threading.Tasks;
using lightspeedAccess;
using Netco.ActionPolicyServices;
using Netco.Utils;

namespace LightspeedAccess.Misc
{
	internal class ActionPolicies
	{
		private static readonly int RetryIntervalSeconds = 45;

		public static ActionPolicy SubmitPolicy( int accountId )
		{
			return ActionPolicy.Handle< Exception >().Retry( 10, ( ex, i ) =>
			{
				LightspeedLogger.Error( ex, string.Format( "Retrying Lightspeed API submit call for the {0} time", i ), accountId );
				if( !LightspeedAuthService.IsUnauthorizedException( ex ) )
					SystemUtil.Sleep( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
			} );
		}

		public static ActionPolicyAsync SubmitPolicyAsync( int accountId )
		{
			return ActionPolicyAsync.Handle< Exception >().RetryAsync( 10, async ( ex, i ) =>
			{
				LightspeedLogger.Error( ex, string.Format( "Retrying Lightspeed API submit call for the {0} time", i ), accountId );
				if( !LightspeedAuthService.IsUnauthorizedException( ex ) )
					await Task.Delay( TimeSpan.FromSeconds( RetryIntervalSeconds ) );
			} );
		}
	}
}