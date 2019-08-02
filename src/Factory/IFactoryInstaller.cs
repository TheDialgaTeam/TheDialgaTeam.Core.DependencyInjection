using Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Core.DependencyInjection.Factory
{
    public interface IFactoryInstaller
    {
        void Install(IServiceCollection serviceCollection);
    }
}