namespace Domain.Entities;
public class SavingsAccount : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty;
    public Guid MemberId { get; set; }
    public decimal Balance { get; set; }
    public decimal CompulsoryWeeklySavings { get; set; } = 20; // Default 20 taka per week
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime OpeningDate { get; set; }
    // Navigation
    public Member Member { get; set; } = null!;
    public ICollection<SavingsTransaction> Transactions { get; set; } = new List<SavingsTransaction>();
}
