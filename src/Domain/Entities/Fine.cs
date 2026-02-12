namespace Domain.Entities;
public class Fine : BaseEntity
{
    public int InstallmentId { get; set; }
    public int LoanId { get; set; }
    public int LateDays { get; set; }
    public decimal FinePerDay { get; set; } = 5; 
    public decimal TotalFine { get; set; }
    public decimal PaidAmount { get; set; }
    public bool IsWaived { get; set; }
    public string? WaivedReason { get; set; }
    public int? WaivedBy { get; set; }
    // Navigation
    public Installment Installment { get; set; } = null!;
    public Loan Loan { get; set; } = null!;
    public User? WaivedByUser { get; set; }
}
