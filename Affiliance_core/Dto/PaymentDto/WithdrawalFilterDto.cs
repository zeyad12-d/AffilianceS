using Affiliance_core.Entites;

namespace Affiliance_core.Dto.PaymentDto
{
    public class WithdrawalFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? MarketerId { get; set; }
        public WithdrawalStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
