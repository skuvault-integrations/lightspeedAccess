using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkuVault.Integrations.Core.DependencyInjection;
using SkuVault.Integrations.Core.Logging;
using SkuVault.Lightspeed.Access.Misc;

namespace SkuVault.Lightspeed.Access.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// This is the main DI registration extension method which will register dependencies that are used by the consumers of this library.
		/// Any dependencies that are needed outside the library should be registered here as well as <see cref="AddLightspeedInternalServices" />.
		/// We register them in <see cref="AddLightspeedInternalServices" /> as well to be used in the factory method to get a single instance if needed.
		/// The internal and public registration should share the same DI lifetime to prevent issues where the instance you get isn't what is expected.
		/// <para></para>
		/// Attaches external logging system (logger factory, providers) with the specified logging level to the internal provider,
		/// so internal services are able to log using the existing logging system.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="addLoggingAction">
		/// this action is used to setup logging withing the access library, if needed to be ignored then pass a no-op function in.
		/// if null is passed in then will throw an exception as we always want some kind of valid value
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="services" />
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="addLoggingAction" />
		/// </exception>
		public static IServiceCollection AddLightspeedServices(this IServiceCollection services, Action<ILoggingBuilder> addLoggingAction)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (addLoggingAction == null)
			{
				throw new ArgumentNullException(nameof(addLoggingAction));
			}

			var lightspeedInternalServiceProvider = CreateInternalServiceProvider(addLoggingAction);

			services.AddSingleton<ILightspeedFactory, LightspeedFactory>(_ =>
				(LightspeedFactory)lightspeedInternalServiceProvider.GetRequiredService<ILightspeedFactory>());

			return services;
		}

		private static ServiceProvider CreateInternalServiceProvider(Action<ILoggingBuilder> addLoggingAction)
		{
			var lightspeedServiceCollection = new ServiceCollection();

			lightspeedServiceCollection.AddLightspeedInternalServices(addLoggingAction);

			return lightspeedServiceCollection.BuildServiceProvider();
		}

		/// <summary>
		/// This should contain DI registrations for all services that need to be registered in the library. We build these internally in the library so that
		/// we can control what DI registrations are exposed outside of the library. The consumer of the library only needs access to a few of these interfaces
		/// and should only register dependencies to be consumed explicitly in <see cref="AddLightspeedServices" />.
		/// <para></para>
		/// Has a possibility of passing existing <see cref="ILoggerFactory" /> service descriptor to be able to use externally setup logging system.
		/// <para></para>
		/// This is internal access modifier instead of private because its used in tests where we need to access internal dependencies that are not available
		/// through the public DI registration via <see cref="AddLightspeedServices" />.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="addLoggingAction"></param>
		internal static IServiceCollection AddLightspeedInternalServices(this IServiceCollection services,
			Action<ILoggingBuilder> addLoggingAction)
		{
			services.AddLogging(addLoggingAction);

			// Custom logging filter should be registered after the HttpClients, so it will replace built-in logging handlers registered when adding clients.
			services.AddIntegrationLogging<LightspeedLogger>(Constants.ChannelName, Constants.VersionInfo, true);
			services.AddSingleton<IIntegrationLogger, LightspeedLogger>();
			services.AddSingleton<ILightspeedFactory, LightspeedFactory>();

			return services;
		}
	}
}