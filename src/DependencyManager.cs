using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Core.DependencyInjection.Factory;
using TheDialgaTeam.Core.DependencyInjection.Service;
using TheDialgaTeam.Core.DependencyInjection.TaskAwaiter;

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
            ServiceCollection.AddInterfacesAndSelfAsSingleton<TaskAwaiterCollection>();
        }

        public void InstallFactory(IFactoryInstaller installer)
        {
            installer.Install(ServiceCollection);
        }

        public void BuildAndExecute(Action<IServiceProvider, Exception> executeFailedAction)
        {
            try
            {
                ServiceProvider = ServiceCollection.BuildServiceProvider();

                var serviceProvider = ServiceProvider;
                var serviceExecutors = serviceProvider.GetServices<IServiceExecutor>();

                foreach (var serviceExecutor in serviceExecutors)
                    serviceExecutor.Execute();

                var taskAwaiter = serviceProvider.GetRequiredService<TaskAwaiterCollection>();
                taskAwaiter.WaitAll();

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