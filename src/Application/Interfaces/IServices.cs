using Domain.Entities;
using Domain.Enums;
namespace Application.Interfaces;
public interface ILoanService
{
    Task<Loan> CreateLoanApplicationAsync(Loan loan);
    Task<Loan> ApproveLoanAsync(int loanId, int approvedBy);
    Task<Loan> DisburseLoanAsync(int loanId);
    Task<Loan?> GetLoanByIdAsync(int loanId);
    Task<List<Loan>> GetLoansByMemberIdAsync(int memberId);
    Task<List<Loan>> GetPendingLoansAsync(int? branchId = null);
    Task<bool> HasActiveLoanAsync(int memberId);
    Task<List<Installment>> GetInstallmentScheduleAsync(int loanId);
}
public interface IPaymentService
{
    Task<Payment> RecordPaymentAsync(Payment payment);
    Task<List<Payment>> GetPaymentsByLoanIdAsync(int loanId);
    Task<List<Payment>> GetPaymentsByMemberIdAsync(int memberId);
    Task<decimal> CalculateFineAsync(int installmentId);
}
public interface ISavingsService
{
    Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account);
    Task<SavingsTransaction> DepositAsync(int accountId, decimal amount, int processedBy, string? remarks = null);
    Task<SavingsTransaction> WithdrawAsync(int accountId, decimal amount, int processedBy, string? remarks = null);
    Task<SavingsAccount?> GetAccountByMemberIdAsync(int memberId);
    Task<decimal> GetBalanceAsync(int accountId);
}
public interface IAccountingService
{
    Task RecordLoanDisbursementAsync(Loan loan, int userId);
    Task RecordLoanRepaymentAsync(Payment payment, int userId);
    Task RecordSavingsTransactionAsync(SavingsTransaction transaction, int userId);
}
public interface IReportService
{
    Task<BranchReportDto> GetBranchReportAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<OverdueReportDto>> GetOverdueReportAsync(int? branchId = null);
    Task<OfficerPerformanceDto> GetOfficerPerformanceAsync(int officerId, DateTime? startDate = null, DateTime? endDate = null);
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
