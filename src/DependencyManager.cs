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

        public void BuildAndExecute(Action<IServiceProvider> preExecuteAction = null, Action<IServiceProvider> postExecuteAction = null, Action<IServiceProvider, Exception> executeFailedAction = null)
        {
            try
            {
                if (_isExecuted) return;
                _isExecuted = true;

                _serviceProvider = _serviceCollection.BuildServiceProvider();

                preExecuteAction?.Invoke(_serviceProvider);

                var serviceExecutors = _serviceProvider.GetServices<IServiceExecutor>();
                var taskAwaiter = _serviceProvider.GetRequiredService<ITaskAwaiter>();

                foreach (var serviceExecutor in serviceExecutors)
                {
                    serviceExecutor.ExecuteService(taskAwaiter);
                }

                (taskAwaiter as TaskCollection)?.WaitAll();

                postExecuteAction?.Invoke(_serviceProvider);
            }
            catch (Exception ex)
            {
                executeFailedAction?.Invoke(_serviceProvider, ex);
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