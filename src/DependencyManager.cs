using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public sealed class DependencyManager : IDisposable
    {
        private readonly IServiceCollection _serviceCollection;

        private ServiceProvider _serviceProvider;

        private Action<IServiceProvider, Exception> _executeFailedAction;

        private bool _isExecuted;

        private bool _isDisposed;

        private IServiceProvider ServiceProvider => _serviceProvider;

        public DependencyManager(IServiceCollection serviceCollection = null)
        {
            _serviceCollection = serviceCollection ?? new ServiceCollection();
            _serviceCollection.AddInterfacesAndSelfAsSingleton<CancellationTokenSource>();
            _serviceCollection.AddInterfacesAndSelfAsSingleton<TaskCollection>();
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
                    return;

                _isExecuted = true;

                _serviceProvider = _serviceCollection.BuildServiceProvider();
                _executeFailedAction = executeFailedAction;

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                var serviceExecutors = _serviceProvider.GetServices<IServiceExecutor>();
                var taskAwaiter = _serviceProvider.GetRequiredService<ITaskAwaiter>();

                foreach (var serviceExecutor in serviceExecutors)
                    serviceExecutor.ExecuteService(taskAwaiter);

                _serviceProvider.GetRequiredService<TaskCollection>().WaitAll();

                var disposableServices = _serviceProvider.GetServices<IDisposable>().Reverse();

                foreach (var disposableService in disposableServices)
                    disposableService.Dispose();

                Dispose();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                executeFailedAction(_serviceProvider, ex);
                Dispose();
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _executeFailedAction(ServiceProvider, e.ExceptionObject as Exception);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _serviceProvider?.Dispose();

            if (!_isExecuted)
                return;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
        }
    }
}