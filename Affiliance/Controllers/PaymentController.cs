using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.PaymentDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for marketer payments, withdrawals, and admin financial actions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public PaymentController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private int? GetCurrentMarketerId()
        {
            var marketerIdClaim = User.FindFirst("marketerId")?.Value;
            return int.TryParse(marketerIdClaim, out var marketerId) ? marketerId : null;
        }

        // Marketer Withdrawal Endpoints

        /// <summary>
        /// Creates a withdrawal request for the current marketer (Marketer only).
        /// </summary>
        /// <param name="dto">Withdrawal details such as amount and payment method.</param>
        /// <returns>Returns the created withdrawal request result.</returns>
        [HttpPost("withdrawal-request")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> CreateWithdrawalRequest([FromBody] CreateWithdrawalRequestDto dto)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized("Marketer ID not found");

            var result = await _servicesManager.PaymentService.CreateWithdrawalRequestAsync(marketerId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets withdrawal history for the current marketer (Marketer only).
        /// </summary>
        /// <param name="filter">Filter options such as status and date range.</param>
        /// <returns>Returns a paginated list of withdrawal requests.</returns>
        [HttpGet("withdrawal-history")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> GetWithdrawalHistory([FromQuery] WithdrawalFilterDto filter)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.GetWithdrawalHistoryAsync(marketerId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets the current marketer balance (Marketer only).
        /// </summary>
        /// <returns>Returns the available balance.</returns>
        [HttpGet("balance")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> GetBalance()
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.GetMarketerBalanceAsync(marketerId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets marketer earnings for a specific campaign (Marketer only).
        /// </summary>
        /// <param name="campaignId">The campaign ID.</param>
        /// <returns>Returns earnings for the given campaign.</returns>
        [HttpGet("earnings/{campaignId}")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> GetEarningsByCampaign(int campaignId)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.GetEarningsByCampaignAsync(marketerId.Value, campaignId);
            return result.Success ?Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets marketer earnings across all campaigns (Marketer only).
        /// </summary>
        /// <returns>Returns aggregated earnings.</returns>
        [HttpGet("earnings")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> GetAllEarnings()
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.GetAllEarningsAsync(marketerId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Payment Method Endpoints

        /// <summary>
        /// Adds a payment method for the current marketer (Marketer only).
        /// </summary>
        /// <param name="dto">Payment method details.</param>
        /// <returns>Returns the created payment method.</returns>
        [HttpPost("payment-method")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> AddPaymentMethod([FromBody] CreatePaymentMethodDto dto)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.AddPaymentMethodAsync(marketerId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets all payment methods for the current marketer (Marketer only).
        /// </summary>
        /// <returns>Returns a list of payment methods.</returns>
        [HttpGet("payment-methods")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.GetPaymentMethodsAsync(marketerId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Updates a payment method for the current marketer (Marketer only).
        /// </summary>
        /// <param name="id">Payment method ID.</param>
        /// <param name="dto">Updated payment method details.</param>
        /// <returns>Returns the updated payment method.</returns>
        [HttpPut("payment-method/{id}")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodDto dto)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.UpdatePaymentMethodAsync(marketerId.Value, id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Deletes a payment method for the current marketer (Marketer only).
        /// </summary>
        /// <param name="id">Payment method ID.</param>
        /// <returns>Returns the deletion result.</returns>
        [HttpDelete("payment-method/{id}")]
        [Authorize(Roles = "Marketer")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            var marketerId = GetCurrentMarketerId();
            if (!marketerId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.DeletePaymentMethodAsync(marketerId.Value, id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Admin Endpoints

        /// <summary>
        /// Gets all withdrawal requests with filters (Admin only).
        /// </summary>
        /// <param name="filter">Filter options such as status and date range.</param>
        /// <returns>Returns a paginated list of withdrawal requests.</returns>
        [HttpGet("admin/withdrawals")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllWithdrawalRequests([FromQuery] WithdrawalFilterDto filter)
        {
            var result = await _servicesManager.PaymentService.GetAllWithdrawalRequestsAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Approves a withdrawal request (Admin only).
        /// </summary>
        /// <param name="id">Withdrawal request ID.</param>
        /// <param name="dto">Processing details such as notes and transfer info.</param>
        /// <returns>Returns the approval result.</returns>
        [HttpPost("admin/withdrawals/{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveWithdrawal(int id, [FromBody] ProcessWithdrawalDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.ApproveWithdrawalAsync(id, userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Rejects a withdrawal request (Admin only).
        /// </summary>
        /// <param name="id">Withdrawal request ID.</param>
        /// <param name="dto">Rejection details.</param>
        /// <returns>Returns the rejection result.</returns>
        [HttpPost("admin/withdrawals/{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectWithdrawal(int id, [FromBody] ProcessWithdrawalDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.PaymentService.RejectWithdrawalAsync(id, userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets financial reports for a date range (Admin only).
        /// </summary>
        /// <param name="startDate">Report start date.</param>
        /// <param name="endDate">Report end date.</param>
        /// <returns>Returns summarized financial reports.</returns>
        [HttpGet("admin/financial-reports")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFinancialReports([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _servicesManager.PaymentService.GetFinancialReportsAsync(startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
