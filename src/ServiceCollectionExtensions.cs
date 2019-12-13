using System;
using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<TService>();

            return AddInterfacesAsSingleton<TService>(serviceCollection);
        }

        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            serviceCollection.AddSingleton(implementationFactory);

            return AddInterfacesAsSingleton<TService>(serviceCollection);
        }

        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection, TService implementationInstance) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            serviceCollection.AddSingleton(implementationInstance);

            return AddInterfacesAsSingleton<TService>(serviceCollection);
        }

        private static IServiceCollection AddInterfacesAsSingleton<TService>(this IServiceCollection serviceCollection) where TService : class
        {
            if (typeof(TService).IsInterface)
                throw new ArgumentException("The selected type is not a class.", nameof(TService));

            var implementationInterfaces = typeof(TService).GetInterfaces();

            foreach (var implementationInterface in implementationInterfaces)
                serviceCollection.AddSingleton(implementationInterface, a => a.GetRequiredService<TService>());

            return serviceCollection;
        }
    }
}