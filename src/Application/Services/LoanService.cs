using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class LoanService : ILoanService
{
    private readonly MicrocreditDbContext _context;
    private readonly IAccountingService _accountingService;
    public LoanService(MicrocreditDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }
    public async Task<Loan> CreateLoanApplicationAsync(Loan loan)
    {
        var member = await _context.Members
            .Include(m => m.SavingsAccount)
            .FirstOrDefaultAsync(m => m.Id == loan.MemberId);
        if (member == null)
            throw new Exception("Member not found");
        if (member.Status != MemberStatus.Active)
            throw new Exception("Member is not active");
        var hasActiveLoan = await HasActiveLoanAsync(loan.MemberId);
        if (hasActiveLoan)
            throw new Exception("Member already has an active loan");
        if (member.SavingsAccount == null || member.SavingsAccount.Balance < 100)
            throw new Exception("Insufficient savings balance. Minimum 100 required.");
        if (loan.GroupId.HasValue)
        {
            var group = await _context.Groups.FindAsync(loan.GroupId.Value);
            if (group != null && group.PerformanceRating < 0.5m)
                throw new Exception("Group performance is below threshold");
        }
        CalculateLoanDetails(loan);
        loan.LoanCode = await GenerateLoanCodeAsync(loan.BranchId);
        loan.ApplicationDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Pending;
        loan.RemainingBalance = loan.TotalPayable;
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }
    public async Task<Loan> ApproveLoanAsync(Guid loanId, Guid approvedBy)
    {
        var loan = await _context.Loans
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null)
            throw new Exception("Loan not found");
        if (loan.Status != LoanStatus.Pending)
            throw new Exception("Loan is not in pending status");
        loan.Status = LoanStatus.Approved;
        loan.ApprovalDate = DateTime.UtcNow;
        loan.ApprovedBy = approvedBy;
        await _context.SaveChangesAsync();
        return loan;
    }
    public async Task<Loan> DisburseLoanAsync(Guid loanId)
    {
        var loan = await _context.Loans.FindAsync(loanId);
        if (loan == null)
            throw new Exception("Loan not found");
        if (loan.Status != LoanStatus.Approved)
            throw new Exception("Loan must be approved before disbursement");
        loan.Status = LoanStatus.Disbursed;
        loan.DisbursementDate = DateTime.UtcNow;
        await GenerateInstallmentScheduleAsync(loan);
        var member = await _context.Members.FindAsync(loan.MemberId);
        if (member != null)
        {
            member.LoanCycle++;
        }
        await _context.SaveChangesAsync();
        if (loan.ApprovedBy.HasValue)
        {
            await _accountingService.RecordLoanDisbursementAsync(loan, loan.ApprovedBy.Value);
        }
        return loan;
    }
    public async Task<Loan?> GetLoanByIdAsync(Guid loanId)
    {
        return await _context.Loans
            .Include(l => l.Member)
            .Include(l => l.Branch)
            .Include(l => l.Installments)
            .FirstOrDefaultAsync(l => l.Id == loanId);
    }
    public async Task<List<Loan>> GetLoansByMemberIdAsync(Guid memberId)
    {
        return await _context.Loans
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.ApplicationDate)
            .ToListAsync();
    }
    public async Task<List<Loan>> GetPendingLoansAsync(Guid? branchId = null)
    {
        var query = _context.Loans
            .Include(l => l.Member)
            .Where(l => l.Status == LoanStatus.Pending);
        if (branchId.HasValue)
        {
            query = query.Where(l => l.BranchId == branchId.Value);
        }
        return await query.ToListAsync();
    }
    public async Task<bool> HasActiveLoanAsync(Guid memberId)
    {
        return await _context.Loans
            .AnyAsync(l => l.MemberId == memberId && 
                          (l.Status == LoanStatus.Approved || 
                           l.Status == LoanStatus.Disbursed || 
                           l.Status == LoanStatus.Active));
    }
    public async Task<List<Installment>> GetInstallmentScheduleAsync(Guid loanId)
    {
        return await _context.Installments
            .Where(i => i.LoanId == loanId)
            .OrderBy(i => i.InstallmentNumber)
            .ToListAsync();
    }
    private void CalculateLoanDetails(Loan loan)
    {
        switch (loan.InterestType)
        {
            case InterestType.Flat:
                loan.TotalInterest = loan.LoanAmount * (loan.InterestRate / 100);
                loan.TotalPayable = loan.LoanAmount + loan.TotalInterest;
                loan.WeeklyInstallment = loan.TotalPayable / loan.DurationInWeeks;
                break;
            case InterestType.ReducingBalance:
                loan.WeeklyInstallment = loan.LoanAmount / loan.DurationInWeeks;
                loan.TotalInterest = 0;
                loan.TotalPayable = loan.LoanAmount;
                break;
            case InterestType.DecliningBalanceEMI:
                decimal weeklyRate = (loan.InterestRate / 100) / 52;
                double rate = (double)weeklyRate;
                int n = loan.DurationInWeeks;
                double principal = (double)loan.LoanAmount;
                double emi = principal * rate * Math.Pow(1 + rate, n) / (Math.Pow(1 + rate, n) - 1);
                loan.WeeklyInstallment = (decimal)emi;
                loan.TotalPayable = loan.WeeklyInstallment * n;
                loan.TotalInterest = loan.TotalPayable - loan.LoanAmount;
                break;
        }
    }
    private async Task GenerateInstallmentScheduleAsync(Loan loan)
    {
        var installments = new List<Installment>();
        var startDate = loan.DisbursementDate!.Value;
        decimal remainingPrincipal = loan.LoanAmount;
        for (int i = 1; i <= loan.DurationInWeeks; i++)
        {
            var installment = new Installment
            {
                Id = Guid.NewGuid(),
                LoanId = loan.Id,
                InstallmentNumber = i,
                DueDate = startDate.AddDays(7 * i),
                Status = InstallmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            switch (loan.InterestType)
            {
                case InterestType.Flat:
                    installment.PrincipalAmount = loan.LoanAmount / loan.DurationInWeeks;
                    installment.InterestAmount = loan.TotalInterest / loan.DurationInWeeks;
                    installment.TotalAmount = loan.WeeklyInstallment;
                    break;
                case InterestType.ReducingBalance:
                    decimal weeklyRate = (loan.InterestRate / 100) / 52;
                    installment.InterestAmount = remainingPrincipal * weeklyRate;
                    installment.PrincipalAmount = loan.WeeklyInstallment - installment.InterestAmount;
                    installment.TotalAmount = loan.WeeklyInstallment;
                    remainingPrincipal -= installment.PrincipalAmount;
                    break;
                case InterestType.DecliningBalanceEMI:
                    decimal weeklyRateEMI = (loan.InterestRate / 100) / 52;
                    installment.InterestAmount = remainingPrincipal * weeklyRateEMI;
                    installment.PrincipalAmount = loan.WeeklyInstallment - installment.InterestAmount;
                    installment.TotalAmount = loan.WeeklyInstallment;
                    remainingPrincipal -= installment.PrincipalAmount;
                    break;
            }
            installment.RemainingAmount = installment.TotalAmount;
            installments.Add(installment);
        }
        await _context.Installments.AddRangeAsync(installments);
        await _context.SaveChangesAsync();
    }
    private async Task<string> GenerateLoanCodeAsync(Guid branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        var branchCode = branch?.BranchCode ?? "BR";
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.Loans.CountAsync(l => l.BranchId == branchId) + 1;
        return $"LN-{branchCode}-{year}-{count:D5}";
    }
}
