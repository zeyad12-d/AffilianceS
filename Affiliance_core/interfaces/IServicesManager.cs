namespace Affiliance_core.interfaces
{
    public interface IServicesManager
    {
        IAiService AiService { get; }
        ICampanyServices CampanyServices { get; }
        ICampaignService CampaignService { get; }
        ICategoryService CategoryService { get; }
        IMarketerService MarketerService { get; }
        IReviewService ReviewService { get; }
        ITrackingLinkService TrackingLinkService { get; }
        IPaymentService PaymentService { get; }
        IComplaintService ComplaintService { get; }
        INotificationService NotificationService { get; }
        IAnalyticsService AnalyticsService { get; }
        IAuditLogService AuditLogService { get; }
        IChatbotService ChatbotService { get; }
    }
}
