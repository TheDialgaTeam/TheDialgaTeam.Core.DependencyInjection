using System;
using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterfacesAsSingleton<TService>(this IServiceCollection serviceCollection) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            var types = typeof(TService).GetInterfaces();

            for (var i = 0; i < types.Length; i++)
            {
                if (i == 0)
                    serviceCollection.AddSingleton(types[i], typeof(TService));
                else
                    serviceCollection.AddSingleton(types[i], a => a.GetRequiredService(types[0]));
            }

            return serviceCollection;
        }

        public static IServiceCollection AddInterfacesAsSingleton<TService>(this IServiceCollection serviceCollection, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            foreach (var type in typeof(TService).GetInterfaces())
                serviceCollection.AddSingleton(type, implementationFactory);

            return serviceCollection;
        }

        public static IServiceCollection AddInterfacesAsSingleton<TService>(this IServiceCollection serviceCollection, TService implementationInstance) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            foreach (var type in typeof(TService).GetInterfaces())
                serviceCollection.AddSingleton(type, implementationInstance);

            return serviceCollection;
        }

        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<TService>();

            foreach (var type in typeof(TService).GetInterfaces())
                serviceCollection.AddSingleton(type, a => a.GetRequiredService<TService>());

            return serviceCollection;
        }

        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            serviceCollection.AddSingleton(implementationFactory);

            foreach (var type in typeof(TService).GetInterfaces())
                serviceCollection.AddSingleton(type, a => a.GetRequiredService<TService>());

            return serviceCollection;
        }

        public static IServiceCollection AddInterfacesAndSelfAsSingleton<TService>(this IServiceCollection serviceCollection, TService implementationInstance) where TService : class
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            serviceCollection.AddSingleton(implementationInstance);

            foreach (var type in typeof(TService).GetInterfaces())
                serviceCollection.AddSingleton(type, a => a.GetRequiredService<TService>());

            return serviceCollection;
        }
    }
}