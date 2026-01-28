using Affiliance_core.interfaces;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class ServicesManager:IServicesManager
    {
        private readonly IServiceFactory _serviceFactory;
        public ServicesManager(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
        }

    }
}
