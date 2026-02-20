using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.PaymentDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, IAuditLogService auditLogService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
        }

        #region Marketer Withdrawal Management

        public async Task<ApiResponse<WithdrawalRequestDto>> CreateWithdrawalRequestAsync(int marketerId, CreateWithdrawalRequestDto dto)
        {
            // Check if marketer exists
            var marketer = await _unitOfWork.Repository<Marketer>()
                .GetQueryable()
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == marketerId);

            if (marketer == null)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Marketer not found");

            // Check payment method exists and belongs to marketer
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>()
                .GetByIdAsync(dto.PaymentMethodId);

            if (paymentMethod == null || paymentMethod.MarketerId != marketerId)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Payment method not found");

            // Get current balance
            var balance = await GetMarketerBalanceAsync(marketerId);
            if (!balance.Success || balance.Data.AvailableBalance < dto.Amount)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Insufficient balance");

            // Minimum withdrawal check
            if (dto.Amount < 10)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Minimum withdrawal amount is 10");

            // Create withdrawal request
            var withdrawal = new WithdrawalRequest
            {
                MarketerId = marketerId,
                Amount = dto.Amount,
                PaymentMethodId = dto.PaymentMethodId,
                Status = WithdrawalStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<WithdrawalRequest>().AddAsync(withdrawal);
            await _unitOfWork.CompleteAsync();

            // Create notification
            await _notificationService.CreateNotificationAsync(
                marketer.UserId,
                "Withdrawal Request Submitted",
                $"Your withdrawal request for {dto.Amount:C} has been submitted and is pending approval.",
                "WithdrawalStatus",
                withdrawal.Id
            );

            // Reload with navigation properties
            withdrawal = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Include(w => w.Marketer).ThenInclude(m => m.User)
                .Include(w => w.PaymentMethod)
                .FirstOrDefaultAsync(w => w.Id == withdrawal.Id);

            var result = _mapper.Map<WithdrawalRequestDto>(withdrawal);
            return ApiResponse<WithdrawalRequestDto>.CreateSuccess(result, "Withdrawal request created successfully");
        }

        public async Task<ApiResponse<PagedResult<WithdrawalRequestDto>>> GetWithdrawalHistoryAsync(int marketerId, WithdrawalFilterDto filter)
        {
            var query = _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.MarketerId == marketerId)
                .Include(w => w.Marketer).ThenInclude(m => m.User)
                .Include(w => w.PaymentMethod)
                .Include(w => w.ProcessedByUser)
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(w => w.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(w => w.RequestedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(w => w.RequestedAt <= filter.EndDate.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(w => w.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(w => w.Amount <= filter.MaxAmount.Value);

            var totalCount = await query.CountAsync();

            var withdrawals = await query
                .OrderByDescending(w => w.RequestedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var withdrawalsDto = _mapper.Map<List<WithdrawalRequestDto>>(withdrawals);
            var pagedResult = new PagedResult<WithdrawalRequestDto>(withdrawalsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<WithdrawalRequestDto>>.CreateSuccess(pagedResult, "Withdrawal history retrieved successfully");
        }

        public async Task<ApiResponse<MarketerBalanceDto>> GetMarketerBalanceAsync(int marketerId)
        {
            // Get all commission payments
            var commissions = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Join(_unitOfWork.Repository<User>().GetQueryable(),
                      p => p.UserId,
                      u => u.Id,
                      (p, u) => new { Payment = p, User = u })
                .Join(_unitOfWork.Repository<Marketer>().GetQueryable(),
                      pu => pu.User.Id,
                      m => m.UserId,
                      (pu, m) => new { pu.Payment, Marketer = m })
                .Where(x => x.Marketer.Id == marketerId && 
                           x.Payment.Type == PaymentType.Commission && 
                           x.Payment.Status == PaymentStatus.Completed)
                .SumAsync(x => x.Payment.Amount);

            // Get all completed withdrawals
            var withdrawn = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.MarketerId == marketerId && w.Status == WithdrawalStatus.Completed)
                .SumAsync(w => w.Amount);

            // Get pending withdrawals
            var pending = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.MarketerId == marketerId && 
                           (w.Status == WithdrawalStatus.Pending || w.Status == WithdrawalStatus.Approved || w.Status == WithdrawalStatus.Processing))
                .SumAsync(w => w.Amount);

            // Get transaction count
            var transactionCount = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Join(_unitOfWork.Repository<User>().GetQueryable(),
                      p => p.UserId,
                      u => u.Id,
                      (p, u) => new { Payment = p, User = u })
                .Join(_unitOfWork.Repository<Marketer>().GetQueryable(),
                      pu => pu.User.Id,
                      m => m.UserId,
                      (pu, m) => new { pu.Payment, Marketer = m })
                .Where(x => x.Marketer.Id == marketerId)
                .CountAsync();

            var balance = new MarketerBalanceDto
            {
                MarketerId = marketerId,
                TotalEarnings = commissions,
                TotalWithdrawn = withdrawn,
                PendingWithdrawals = pending,
                AvailableBalance = commissions - withdrawn - pending,
                TotalTransactions = transactionCount,
                LastUpdated = DateTime.UtcNow
            };

            return ApiResponse<MarketerBalanceDto>.CreateSuccess(balance, "Balance retrieved successfully");
        }

        public async Task<ApiResponse<CampaignEarningsDto>> GetEarningsByCampaignAsync(int marketerId, int campaignId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<CampaignEarningsDto>.CreateFail("Marketer not found");

            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null)
                return ApiResponse<CampaignEarningsDto>.CreateFail("Campaign not found");

            // Get tracking links for this marketer and campaign
            var trackingLinks = await _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(tl => tl.MarketerId == marketerId && tl.CampaignId == campaignId)
                .Include(tl => tl.PerformanceLogs)
                .ToListAsync();

            var totalEarnings = trackingLinks.Sum(tl => tl.Earnings);
            var totalConversions = trackingLinks.Sum(tl => tl.Conversions);
            var totalClicks = trackingLinks.Sum(tl => tl.Clicks);
            var conversionRate = totalClicks > 0 ? (decimal)totalConversions / totalClicks * 100 : 0;

            var firstLog = trackingLinks
                .SelectMany(tl => tl.PerformanceLogs)
                .Where(pl => pl.EventType == PerformanceEventType.Conversion)
                .OrderBy(pl => pl.Timestamp)
                .FirstOrDefault();

            var lastLog = trackingLinks
                .SelectMany(tl => tl.PerformanceLogs)
                .Where(pl => pl.EventType == PerformanceEventType.Conversion)
                .OrderByDescending(pl => pl.Timestamp)
                .FirstOrDefault();

            var result = new CampaignEarningsDto
            {
                CampaignId = campaignId,
                CampaignTitle = campaign.Title,
                TotalEarnings = totalEarnings,
                TotalConversions = totalConversions,
                TotalClicks = totalClicks,
                ConversionRate = conversionRate,
                FirstEarningDate = firstLog?.Timestamp ?? DateTime.UtcNow,
                LastEarningDate = lastLog?.Timestamp ?? DateTime.UtcNow
            };

            return ApiResponse<CampaignEarningsDto>.CreateSuccess(result, "Campaign earnings retrieved successfully");
        }

        public async Task<ApiResponse<List<CampaignEarningsDto>>> GetAllEarningsAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<List<CampaignEarningsDto>>.CreateFail("Marketer not found");

            // Get all campaigns with tracking links
            var trackingLinks = await _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(tl => tl.MarketerId == marketerId)
                .Include(tl => tl.Campaign)
                .Include(tl => tl.PerformanceLogs)
                .ToListAsync();

            var earnings = trackingLinks
                .GroupBy(tl => new { tl.CampaignId, tl.Campaign.Title })
                .Select(g => {
                    var conversions = g.Sum(tl => tl.Conversions);
                    var clicks = g.Sum(tl => tl.Clicks);
                    var firstLog = g.SelectMany(tl => tl.PerformanceLogs)
                        .Where(pl => pl.EventType == PerformanceEventType.Conversion)
                        .OrderBy(pl => pl.Timestamp)
                        .FirstOrDefault();
                    var lastLog = g.SelectMany(tl => tl.PerformanceLogs)
                        .Where(pl => pl.EventType == PerformanceEventType.Conversion)
                        .OrderByDescending(pl => pl.Timestamp)
                        .FirstOrDefault();

                    return new CampaignEarningsDto
                    {
                        CampaignId = g.Key.CampaignId,
                        CampaignTitle = g.Key.Title,
                        TotalEarnings = g.Sum(tl => tl.Earnings),
                        TotalConversions = conversions,
                        TotalClicks = clicks,
                        ConversionRate = clicks > 0 ? (decimal)conversions / clicks * 100 : 0,
                        FirstEarningDate = firstLog?.Timestamp ?? DateTime.UtcNow,
                        LastEarningDate = lastLog?.Timestamp ?? DateTime.UtcNow
                    };
                })
                .OrderByDescending(e => e.TotalEarnings)
                .ToList();

            return ApiResponse<List<CampaignEarningsDto>>.CreateSuccess(earnings, "All earnings retrieved successfully");
        }

        #endregion

        #region Payment Method Management

        public async Task<ApiResponse<PaymentMethodDto>> AddPaymentMethodAsync(int marketerId, CreatePaymentMethodDto dto)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<PaymentMethodDto>.CreateFail("Marketer not found");

            // If setting as default, unset other defaults
            if (dto.SetAsDefault)
            {
                var existingMethods = await _unitOfWork.Repository<PaymentMethod>()
                    .GetQueryable()
                    .Where(pm => pm.MarketerId == marketerId && pm.IsDefault)
                    .ToListAsync();

                foreach (var method in existingMethods)
                {
                    method.IsDefault = false;
                    _unitOfWork.Repository<PaymentMethod>().Update(method);
                }
            }

            var paymentMethod = new PaymentMethod
            {
                MarketerId = marketerId,
                Type = dto.Type,
                AccountDetails = dto.AccountDetails,
                AccountHolderName = dto.AccountHolderName,
                IsDefault = dto.SetAsDefault,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PaymentMethod>().AddAsync(paymentMethod);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<PaymentMethodDto>(paymentMethod);
            return ApiResponse<PaymentMethodDto>.CreateSuccess(result, "Payment method added successfully");
        }

        public async Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(int marketerId, int paymentMethodId, UpdatePaymentMethodDto dto)
        {
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>()
                .GetQueryable()
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.MarketerId == marketerId);

            if (paymentMethod == null)
                return ApiResponse<PaymentMethodDto>.CreateFail("Payment method not found");

            if (dto.AccountHolderName != null)
                paymentMethod.AccountHolderName = dto.AccountHolderName;

            if (dto.SetAsDefault.HasValue && dto.SetAsDefault.Value)
            {
                // Unset other defaults
                var existingMethods = await _unitOfWork.Repository<PaymentMethod>()
                    .GetQueryable()
                    .Where(pm => pm.MarketerId == marketerId && pm.IsDefault && pm.Id != paymentMethodId)
                    .ToListAsync();

                foreach (var method in existingMethods)
                {
                    method.IsDefault = false;
                    _unitOfWork.Repository<PaymentMethod>().Update(method);
                }

                paymentMethod.IsDefault = true;
            }

            _unitOfWork.Repository<PaymentMethod>().Update(paymentMethod);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<PaymentMethodDto>(paymentMethod);
            return ApiResponse<PaymentMethodDto>.CreateSuccess(result, "Payment method updated successfully");
        }

        public async Task<ApiResponse<List<PaymentMethodDto>>> GetPaymentMethodsAsync(int marketerId)
        {
            var methods = await _unitOfWork.Repository<PaymentMethod>()
                .GetQueryable()
                .Where(pm => pm.MarketerId == marketerId)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .ToListAsync();

            var result = _mapper.Map<List<PaymentMethodDto>>(methods);
            return ApiResponse<List<PaymentMethodDto>>.CreateSuccess(result, "Payment methods retrieved successfully");
        }

        public async Task<ApiResponse<bool>> SetDefaultPaymentMethodAsync(int marketerId, int paymentMethodId)
        {
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>()
                .GetQueryable()
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.MarketerId == marketerId);

            if (paymentMethod == null)
                return ApiResponse<bool>.CreateFail("Payment method not found");

            // Unset other defaults
            var existingMethods = await _unitOfWork.Repository<PaymentMethod>()
                .GetQueryable()
                .Where(pm => pm.MarketerId == marketerId && pm.IsDefault)
                .ToListAsync();

            foreach (var method in existingMethods)
            {
                method.IsDefault = false;
                _unitOfWork.Repository<PaymentMethod>().Update(method);
            }

            paymentMethod.IsDefault = true;
            _unitOfWork.Repository<PaymentMethod>().Update(paymentMethod);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Default payment method set successfully");
        }

        public async Task<ApiResponse<bool>> DeletePaymentMethodAsync(int marketerId, int paymentMethodId)
        {
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>()
                .GetQueryable()
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.MarketerId == marketerId);

            if (paymentMethod == null)
                return ApiResponse<bool>.CreateFail("Payment method not found");

            // Check if used in pending/processing withdrawals
            var hasActiveWithdrawals = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .AnyAsync(w => w.PaymentMethodId == paymentMethodId && 
                              (w.Status == WithdrawalStatus.Pending || 
                               w.Status == WithdrawalStatus.Approved || 
                               w.Status == WithdrawalStatus.Processing));

            if (hasActiveWithdrawals)
                return ApiResponse<bool>.CreateFail("Cannot delete payment method with active withdrawal requests");

            _unitOfWork.Repository<PaymentMethod>().Delete(paymentMethod);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Payment method deleted successfully");
        }

        #endregion

        #region Admin Withdrawal Management

        public async Task<ApiResponse<PagedResult<WithdrawalRequestDto>>> GetAllWithdrawalRequestsAsync(WithdrawalFilterDto filter)
        {
            var query = _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Include(w => w.Marketer).ThenInclude(m => m.User)
                .Include(w => w.PaymentMethod)
                .Include(w => w.ProcessedByUser)
                .AsQueryable();

            // Apply filters
            if (filter.MarketerId.HasValue)
                query = query.Where(w => w.MarketerId == filter.MarketerId.Value);

            if (filter.Status.HasValue)
                query = query.Where(w => w.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(w => w.RequestedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(w => w.RequestedAt <= filter.EndDate.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(w => w.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(w => w.Amount <= filter.MaxAmount.Value);

            var totalCount = await query.CountAsync();

            var withdrawals = await query
                .OrderByDescending(w => w.RequestedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var withdrawalsDto = _mapper.Map<List<WithdrawalRequestDto>>(withdrawals);
            var pagedResult = new PagedResult<WithdrawalRequestDto>(withdrawalsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<WithdrawalRequestDto>>.CreateSuccess(pagedResult, "All withdrawal requests retrieved successfully");
        }

        public async Task<ApiResponse<WithdrawalRequestDto>> ApproveWithdrawalAsync(int withdrawalId, int adminId, ProcessWithdrawalDto dto)
        {
            var withdrawal = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Include(w => w.Marketer).ThenInclude(m => m.User)
                .Include(w => w.PaymentMethod)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Withdrawal request not found");

            if (withdrawal.Status != WithdrawalStatus.Pending)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Only pending withdrawals can be approved");

            // Check balance again
            var balance = await GetMarketerBalanceAsync(withdrawal.MarketerId);
            if (!balance.Success || balance.Data.AvailableBalance < withdrawal.Amount)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Insufficient balance");

            withdrawal.Status = WithdrawalStatus.Approved;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = adminId;
            withdrawal.AdminNotes = dto.AdminNotes;

            _unitOfWork.Repository<WithdrawalRequest>().Update(withdrawal);

            // Create payment record
            var payment = new Payment
            {
                UserId = withdrawal.Marketer.UserId,
                Amount = withdrawal.Amount,
                Type = PaymentType.Withdrawal,
                Status = PaymentStatus.Processing,
                TransactionId = dto.TransactionId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            // Log audit
            await _auditLogService.LogActionAsync(
                adminId,
                "Withdrawal Approved",
                "WithdrawalRequest",
                withdrawalId,
                null,
                $"Amount: {withdrawal.Amount:C}",
                null,
                null
            );

            // Notify marketer
            await _notificationService.CreateNotificationAsync(
                withdrawal.Marketer.UserId,
                "Withdrawal Approved",
                $"Your withdrawal request for {withdrawal.Amount:C} has been approved and is being processed.",
                "WithdrawalStatus",
                withdrawalId
            );

            var result = _mapper.Map<WithdrawalRequestDto>(withdrawal);
            return ApiResponse<WithdrawalRequestDto>.CreateSuccess(result, "Withdrawal approved successfully");
        }

        public async Task<ApiResponse<WithdrawalRequestDto>> RejectWithdrawalAsync(int withdrawalId, int adminId, ProcessWithdrawalDto dto)
        {
            var withdrawal = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Include(w => w.Marketer).ThenInclude(m => m.User)
                .Include(w => w.PaymentMethod)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId);

            if (withdrawal == null)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Withdrawal request not found");

            if (withdrawal.Status != WithdrawalStatus.Pending)
                return ApiResponse<WithdrawalRequestDto>.CreateFail("Only pending withdrawals can be rejected");

            withdrawal.Status = WithdrawalStatus.Rejected;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            withdrawal.ProcessedBy = adminId;
            withdrawal.RejectionReason = dto.Reason;
            withdrawal.AdminNotes = dto.AdminNotes;

            _unitOfWork.Repository<WithdrawalRequest>().Update(withdrawal);
            await _unitOfWork.CompleteAsync();

            // Log audit
            await _auditLogService.LogActionAsync(
                adminId,
                "Withdrawal Rejected",
                "WithdrawalRequest",
                withdrawalId,
                null,
                $"Reason: {dto.Reason}",
                null,
                null
            );

            // Notify marketer
            await _notificationService.CreateNotificationAsync(
                withdrawal.Marketer.UserId,
                "Withdrawal Rejected",
                $"Your withdrawal request for {withdrawal.Amount:C} has been rejected. Reason: {dto.Reason}",
                "WithdrawalStatus",
                withdrawalId
            );

            var result = _mapper.Map<WithdrawalRequestDto>(withdrawal);
            return ApiResponse<WithdrawalRequestDto>.CreateSuccess(result, "Withdrawal rejected successfully");
        }

        public async Task<ApiResponse<object>> GetFinancialReportsAsync(DateTime startDate, DateTime endDate)
        {
            var totalWithdrawals = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.RequestedAt >= startDate && w.RequestedAt <= endDate)
                .SumAsync(w => w.Amount);

            var completedWithdrawals = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.RequestedAt >= startDate && w.RequestedAt <= endDate && w.Status == WithdrawalStatus.Completed)
                .SumAsync(w => w.Amount);

            var pendingWithdrawals = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.Status == WithdrawalStatus.Pending || w.Status == WithdrawalStatus.Approved)
                .SumAsync(w => w.Amount);

            var totalCommissions = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.Type == PaymentType.Commission && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            var report = new
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalWithdrawals = totalWithdrawals,
                CompletedWithdrawals = completedWithdrawals,
                PendingWithdrawals = pendingWithdrawals,
                TotalCommissions = totalCommissions,
                NetBalance = totalCommissions - completedWithdrawals
            };

            return ApiResponse<object>.CreateSuccess(report, "Financial report generated successfully");
        }

        #endregion

        #region Payment Transaction Management

        public async Task<ApiResponse<PaymentDto>> RecordPaymentAsync(CreatePaymentDto dto)
        {
            var payment = _mapper.Map<Payment>(dto);
            payment.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<PaymentDto>(payment);
            return ApiResponse<PaymentDto>.CreateSuccess(result, "Payment recorded successfully");
        }

        public async Task<ApiResponse<PagedResult<PaymentDto>>> GetPaymentHistoryAsync(PaymentFilterDto filter)
        {
            var query = _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Include(p => p.User)
                .Include(p => p.Campaign)
                .AsQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
                query = query.Where(p => p.UserId == filter.UserId.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(p => p.CampaignId == filter.CampaignId.Value);

            if (filter.Type.HasValue)
                query = query.Where(p => p.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.EndDate.Value);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var paymentsDto = _mapper.Map<List<PaymentDto>>(payments);
            var pagedResult = new PagedResult<PaymentDto>(paymentsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<PaymentDto>>.CreateSuccess(pagedResult, "Payment history retrieved successfully");
        }

        #endregion
    }
}
