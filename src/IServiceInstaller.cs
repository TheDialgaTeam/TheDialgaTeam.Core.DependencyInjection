using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public interface IServiceInstaller
    {
        void InstallService(IServiceCollection serviceCollection);
    }
}