using Affiliance_core.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class ServiceFactory: IServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public ServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public T CreateService<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
