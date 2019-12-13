using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Core.DependencyInjection.Executor;

namespace TheDialgaTeam.Core.DependencyInjection.Installer
{
    public class WaitForCancellationServiceInstaller : IServiceInstaller
    {
        public void InstallService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddInterfacesAndSelfAsSingleton<WaitForCancellationServiceExecutor>();
        }
    }
}