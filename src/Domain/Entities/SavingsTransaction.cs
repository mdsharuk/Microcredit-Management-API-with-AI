namespace Domain.Entities;
public class SavingsTransaction : BaseEntity
{
    public Guid SavingsAccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Reference { get; set; }
    public Guid ProcessedBy { get; set; }
    public string? Remarks { get; set; }
    // Navigation
    public SavingsAccount SavingsAccount { get; set; } = null!;
    public User Processor { get; set; } = null!;
}
