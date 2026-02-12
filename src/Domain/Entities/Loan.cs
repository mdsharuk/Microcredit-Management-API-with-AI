using Domain.Enums;
namespace Domain.Entities;
public class Loan : BaseEntity
{
    public string LoanCode { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public int BranchId { get; set; }
    public int? GroupId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; } // Yearly percentage
    public InterestType InterestType { get; set; } = InterestType.Flat;
    public int DurationInWeeks { get; set; }
    public string Purpose { get; set; } = string.Empty;
    // Calculated fields
    public decimal TotalInterest { get; set; }
    public decimal TotalPayable { get; set; }
    public decimal WeeklyInstallment { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public int? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    // Tracking
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public int PaidInstallments { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    // Navigation
    public Member Member { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Group? Group { get; set; }
    public User? Approver { get; set; }
    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
