using Domain.Enums;
namespace Domain.Entities;
public class Payment : BaseEntity
{
    public string PaymentCode { get; set; } = string.Empty;
    public int LoanId { get; set; }
    public int MemberId { get; set; }
    public int? InstallmentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal FinePaid { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public int CollectedBy { get; set; }
    public string? Remarks { get; set; }
    // Navigation
    public Loan Loan { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public Installment? Installment { get; set; }
    public User Collector { get; set; } = null!;
}
