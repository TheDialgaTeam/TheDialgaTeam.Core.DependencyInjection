using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public sealed class DependencyManager : IDisposable
    {
        private ServiceCollection ServiceCollection { get; }

        private ServiceProvider ServiceProvider { get; set; }

        public DependencyManager()
        {
            ServiceCollection = new ServiceCollection();
            ServiceCollection.AddSingleton(new CancellationTokenSource());
            ServiceCollection.AddInterfacesAndSelfAsSingleton<TaskCollection>();
        }

        public void InstallService(IServiceInstaller serviceInstaller)
        {
            serviceInstaller.InstallService(ServiceCollection);
        }

        public void InstallService(Action<IServiceCollection> installServiceAction)
        {
            installServiceAction(ServiceCollection);
        }

        public void BuildAndExecute(Action<IServiceProvider, Exception> executeFailedAction)
        {
            try
            {
                ServiceProvider = ServiceCollection.BuildServiceProvider();

                var serviceProvider = ServiceProvider;
                var serviceExecutors = serviceProvider.GetServices<IServiceExecutor>();
                var taskAwaiter = serviceProvider.GetService<ITaskAwaiter>();

                foreach (var serviceExecutor in serviceExecutors)
                    serviceExecutor.ExecuteService(taskAwaiter);

                var taskCollection = serviceProvider.GetRequiredService<TaskCollection>();
                taskCollection.WaitAll();

                var disposableServices = serviceProvider.GetServices<IDisposable>().Reverse();

                foreach (var disposableService in disposableServices)
                    disposableService.Dispose();

                Dispose();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                executeFailedAction(ServiceProvider, ex);
            }
        }

        public void Dispose()
        {
            ServiceProvider?.Dispose();
        }
    }
}