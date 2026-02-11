using Domain.Enums;
namespace Domain.Entities;
public class Installment : BaseEntity
{
    public Guid LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public DateTime? PaymentDate { get; set; }
    public int LateDays { get; set; }
    public decimal FineAmount { get; set; }
    // Navigation
    public Loan Loan { get; set; } = null!;
}
