using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.PaymentDto;

namespace Affiliance_core.interfaces
{
    public interface IPaymentService
    {
        // ========================================
        // Marketer Withdrawal Management
        // ========================================
        
        /// <summary>
        /// Create a new withdrawal request for marketer
        /// </summary>
        Task<ApiResponse<WithdrawalRequestDto>> CreateWithdrawalRequestAsync(int marketerId, CreateWithdrawalRequestDto dto);
        
        /// <summary>
        /// Get marketer's withdrawal history
        /// </summary>
        Task<ApiResponse<PagedResult<WithdrawalRequestDto>>> GetWithdrawalHistoryAsync(int marketerId, WithdrawalFilterDto filter);
        
        /// <summary>
        /// Get marketer's current balance details
        /// </summary>
        Task<ApiResponse<MarketerBalanceDto>> GetMarketerBalanceAsync(int marketerId);
        
        /// <summary>
        /// Get marketer's earnings by campaign
        /// </summary>
        Task<ApiResponse<CampaignEarningsDto>> GetEarningsByCampaignAsync(int marketerId, int campaignId);
        
        /// <summary>
        /// Get all earnings for a marketer grouped by campaign
        /// </summary>
        Task<ApiResponse<List<CampaignEarningsDto>>> GetAllEarningsAsync(int marketerId);

        // ========================================
        // Payment Method Management
        // ========================================
        
        /// <summary>
        /// Add a new payment method for marketer
        /// </summary>
        Task<ApiResponse<PaymentMethodDto>> AddPaymentMethodAsync(int marketerId, CreatePaymentMethodDto dto);
        
        /// <summary>
        /// Update payment method
        /// </summary>
        Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(int marketerId, int paymentMethodId, UpdatePaymentMethodDto dto);
        
        /// <summary>
        /// Get all payment methods for marketer
        /// </summary>
        Task<ApiResponse<List<PaymentMethodDto>>> GetPaymentMethodsAsync(int marketerId);
        
        /// <summary>
        /// Set payment method as default
        /// </summary>
        Task<ApiResponse<bool>> SetDefaultPaymentMethodAsync(int marketerId, int paymentMethodId);
        
        /// <summary>
        /// Delete payment method
        /// </summary>
        Task<ApiResponse<bool>> DeletePaymentMethodAsync(int marketerId, int paymentMethodId);

        // ========================================
        // Admin Withdrawal Management
        // ========================================
        
        /// <summary>
        /// Get all withdrawal requests with filtering (Admin)
        /// </summary>
        Task<ApiResponse<PagedResult<WithdrawalRequestDto>>> GetAllWithdrawalRequestsAsync(WithdrawalFilterDto filter);
        
        /// <summary>
        /// Approve withdrawal request (Admin)
        /// </summary>
        Task<ApiResponse<WithdrawalRequestDto>> ApproveWithdrawalAsync(int withdrawalId, int adminId, ProcessWithdrawalDto dto);
        
        /// <summary>
        /// Reject withdrawal request (Admin)
        /// </summary>
        Task<ApiResponse<WithdrawalRequestDto>> RejectWithdrawalAsync(int withdrawalId, int adminId, ProcessWithdrawalDto dto);
        
        /// <summary>
        /// Get financial reports (Admin)
        /// </summary>
        Task<ApiResponse<object>> GetFinancialReportsAsync(DateTime startDate, DateTime endDate);

        // ========================================
        // Payment Transaction Management
        // ========================================
        
        /// <summary>
        /// Record a new payment transaction (internal use)
        /// </summary>
        Task<ApiResponse<PaymentDto>> RecordPaymentAsync(CreatePaymentDto dto);
        
        /// <summary>
        /// Get payment history
        /// </summary>
        Task<ApiResponse<PagedResult<PaymentDto>>> GetPaymentHistoryAsync(PaymentFilterDto filter);
    }
}
