using Affiliance_core.interfaces;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class ServicesManager:IServicesManager
    {
        private readonly IServiceFactory _serviceFactory;
        private IAiService _aiService;

        public ServicesManager(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
        }

        public IAiService AiService
        {
            get
            {
                if (_aiService == null)
                {
                    _aiService = _serviceFactory.CreateService<IAiService>();
                }
                return _aiService;
            }
        }
    }
}
