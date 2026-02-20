using Affiliance_core.interfaces;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class ServicesManager:IServicesManager
    {
        private readonly IServiceFactory _serviceFactory;
        private IAiService _aiService;
        private readonly Lazy<ICampanyServices> _campanyServices;
        private readonly Lazy<ICampaignService> _campaignService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly Lazy<IMarketerService> _marketerService;
        private readonly Lazy<IReviewService> _reviewService;
        private readonly Lazy<ITrackingLinkService> _trackingLinkService;
        private readonly Lazy<IPaymentService> _paymentService;
        private readonly Lazy<IComplaintService> _complaintService;
        private readonly Lazy<INotificationService> _notificationService;
        private readonly Lazy<IAnalyticsService> _analyticsService;
        private readonly Lazy<IAuditLogService> _auditLogService;
        private readonly Lazy<IChatbotService> _chatbotService;

        public ServicesManager(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _campanyServices = new Lazy<ICampanyServices>(() => _serviceFactory.CreateService<ICampanyServices>());
            _campaignService = new Lazy<ICampaignService>(() => _serviceFactory.CreateService<ICampaignService>());
            _categoryService = new Lazy<ICategoryService>(() => _serviceFactory.CreateService<ICategoryService>());
            _marketerService = new Lazy<IMarketerService>(() => _serviceFactory.CreateService<IMarketerService>());
            _reviewService = new Lazy<IReviewService>(() => _serviceFactory.CreateService<IReviewService>());
            _trackingLinkService = new Lazy<ITrackingLinkService>(() => _serviceFactory.CreateService<ITrackingLinkService>());
            _paymentService = new Lazy<IPaymentService>(() => _serviceFactory.CreateService<IPaymentService>());
            _complaintService = new Lazy<IComplaintService>(() => _serviceFactory.CreateService<IComplaintService>());
            _notificationService = new Lazy<INotificationService>(() => _serviceFactory.CreateService<INotificationService>());
            _analyticsService = new Lazy<IAnalyticsService>(() => _serviceFactory.CreateService<IAnalyticsService>());
            _auditLogService = new Lazy<IAuditLogService>(() => _serviceFactory.CreateService<IAuditLogService>());
            _chatbotService = new Lazy<IChatbotService>(() => _serviceFactory.CreateService<IChatbotService>());
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
        public ICampaignService CampaignService => _campaignService.Value;
        public ICategoryService CategoryService => _categoryService.Value;
        public IMarketerService MarketerService => _marketerService.Value;
        public IReviewService ReviewService => _reviewService.Value;
        public ITrackingLinkService TrackingLinkService => _trackingLinkService.Value;
        public IPaymentService PaymentService => _paymentService.Value;
        public IComplaintService ComplaintService => _complaintService.Value;
        public INotificationService NotificationService => _notificationService.Value;
        public IAnalyticsService AnalyticsService => _analyticsService.Value;
        public IAuditLogService AuditLogService => _auditLogService.Value;
        public IChatbotService ChatbotService => _chatbotService.Value;
    }
}
