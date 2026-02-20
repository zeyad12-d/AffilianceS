namespace Affiliance_core.Dto.PaymentDto
{
    public class MarketerBalanceDto
    {
        public int MarketerId { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public decimal PendingWithdrawals { get; set; }
        public decimal AvailableBalance { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
