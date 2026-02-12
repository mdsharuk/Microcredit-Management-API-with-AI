using Domain.Enums;
namespace Domain.Entities;
public class Transaction : BaseEntity
{
    public string TransactionCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public int DebitAccountId { get; set; }
    public int CreditAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int? LoanId { get; set; }
    public int? PaymentId { get; set; }
    public int CreatedByUserId { get; set; }
    // Navigation
    public LedgerAccount DebitAccount { get; set; } = null!;
    public LedgerAccount CreditAccount { get; set; } = null!;
    public Loan? Loan { get; set; }
    public Payment? Payment { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
