using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public class DependencyManager : IDisposable
    {
        private readonly IServiceCollection _serviceCollection;

        private ServiceProvider _serviceProvider;

        private bool _isExecuted;

        private bool _isDisposed;

        public DependencyManager()
        {
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddSingleton<CancellationTokenSource>();
            _serviceCollection.AddSingleton<ITaskAwaiter, TaskCollection>();
        }

        public void InstallService(IServiceInstaller serviceInstaller)
        {
            serviceInstaller.InstallService(_serviceCollection);
        }

        public void InstallService(Action<IServiceCollection> installServiceAction)
        {
            installServiceAction(_serviceCollection);
        }

        public void BuildAndExecute(Action<IServiceProvider, Exception> executeFailedAction)
        {
            try
            {
                if (_isExecuted)
                {
                    return;
                }

                _isExecuted = true;

                _serviceProvider = _serviceCollection.BuildServiceProvider();

                var serviceExecutors = _serviceProvider.GetServices<IServiceExecutor>();
                var taskAwaiter = _serviceProvider.GetRequiredService<ITaskAwaiter>();

                foreach (var serviceExecutor in serviceExecutors)
                {
                    serviceExecutor.ExecuteService(taskAwaiter);
                }

                (taskAwaiter as TaskCollection)?.WaitAll();
            }
            catch (Exception ex)
            {
                executeFailedAction(_serviceProvider, ex);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _serviceProvider?.Dispose();
        }
    }
}