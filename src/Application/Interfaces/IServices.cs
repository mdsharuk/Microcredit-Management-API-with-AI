using Domain.Entities;
using Domain.Enums;
namespace Application.Interfaces;
public interface ILoanService
{
    Task<Loan> CreateLoanApplicationAsync(Loan loan);
    Task<Loan> ApproveLoanAsync(Guid loanId, Guid approvedBy);
    Task<Loan> DisburseLoanAsync(Guid loanId);
    Task<Loan?> GetLoanByIdAsync(Guid loanId);
    Task<List<Loan>> GetLoansByMemberIdAsync(Guid memberId);
    Task<List<Loan>> GetPendingLoansAsync(Guid? branchId = null);
    Task<bool> HasActiveLoanAsync(Guid memberId);
    Task<List<Installment>> GetInstallmentScheduleAsync(Guid loanId);
}
public interface IPaymentService
{
    Task<Payment> RecordPaymentAsync(Payment payment);
    Task<List<Payment>> GetPaymentsByLoanIdAsync(Guid loanId);
    Task<List<Payment>> GetPaymentsByMemberIdAsync(Guid memberId);
    Task<decimal> CalculateFineAsync(Guid installmentId);
}
public interface ISavingsService
{
    Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account);
    Task<SavingsTransaction> DepositAsync(Guid accountId, decimal amount, Guid processedBy, string? remarks = null);
    Task<SavingsTransaction> WithdrawAsync(Guid accountId, decimal amount, Guid processedBy, string? remarks = null);
    Task<SavingsAccount?> GetAccountByMemberIdAsync(Guid memberId);
    Task<decimal> GetBalanceAsync(Guid accountId);
}
public interface IAccountingService
{
    Task RecordLoanDisbursementAsync(Loan loan, Guid userId);
    Task RecordLoanRepaymentAsync(Payment payment, Guid userId);
    Task RecordSavingsTransactionAsync(SavingsTransaction transaction, Guid userId);
}
public interface IReportService
{
    Task<BranchReportDto> GetBranchReportAsync(Guid branchId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<OverdueReportDto>> GetOverdueReportAsync(Guid? branchId = null);
    Task<OfficerPerformanceDto> GetOfficerPerformanceAsync(Guid officerId, DateTime? startDate = null, DateTime? endDate = null);
}
// DTOs for reports
public class BranchReportDto
{
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalDisbursed { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalOverdue { get; set; }
    public int ActiveLoans { get; set; }
    public int TotalMembers { get; set; }
}
public class OverdueReportDto
{
    public string MemberName { get; set; } = string.Empty;
    public string LoanCode { get; set; } = string.Empty;
    public decimal OverdueAmount { get; set; }
    public int LateDays { get; set; }
    public DateTime DueDate { get; set; }
}
public class OfficerPerformanceDto
{
    public string OfficerName { get; set; } = string.Empty;
    public decimal TotalCollected { get; set; }
    public decimal CollectionTarget { get; set; }
    public decimal RecoveryRate { get; set; }
    public int AssignedGroups { get; set; }
}
