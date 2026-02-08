using Affiliance_core.interfaces;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class ServicesManager:IServicesManager
    {
        private readonly IServiceFactory _serviceFactory;
        private IAiService _aiService;
        private readonly Lazy<ICampanyServices> _campanyServices;
        private readonly Lazy<ICategoryService> _categoryService;

        public ServicesManager(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _campanyServices = new Lazy<ICampanyServices>(() => _serviceFactory.CreateService<ICampanyServices>());
            _categoryService = new Lazy<ICategoryService>(() => _serviceFactory.CreateService<ICategoryService>());
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
        public ICampanyServices CampanyServices => _campanyServices.Value;
        public ICategoryService CategoryService => _categoryService.Value;
    }
}
