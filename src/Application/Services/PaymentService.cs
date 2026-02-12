using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class PaymentService : IPaymentService
{
    private readonly MicrocreditDbContext _context;
    private readonly IAccountingService _accountingService;
    private readonly ISavingsService _savingsService;
    public PaymentService(MicrocreditDbContext context, IAccountingService accountingService, ISavingsService savingsService)
    {
        _context = context;
        _accountingService = accountingService;
        _savingsService = savingsService;
    }
    public async Task<Payment> RecordPaymentAsync(Payment payment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var loan = await _context.Loans
                .Include(l => l.Installments)
                .FirstOrDefaultAsync(l => l.Id == payment.LoanId);
            if (loan == null)
                throw new Exception("Loan not found");
            if (!payment.InstallmentId.HasValue)
            {
                var nextInstallment = loan.Installments
                    .Where(i => i.Status == InstallmentStatus.Pending || i.Status == InstallmentStatus.Partial)
                    .OrderBy(i => i.InstallmentNumber)
                    .FirstOrDefault();
                if (nextInstallment != null)
                {
                    payment.InstallmentId = nextInstallment.Id;
                }
            }
            if (payment.InstallmentId.HasValue)
            {
                var fine = await CalculateFineAsync(payment.InstallmentId.Value);
                var installment = await _context.Installments.FindAsync(payment.InstallmentId.Value);
                if (installment != null)
                {
                    installment.FineAmount = fine;
                    installment.LateDays = (int)(DateTime.UtcNow - installment.DueDate).TotalDays;
                    if (installment.LateDays < 0)
                        installment.LateDays = 0;
                }
            }
            decimal remainingAmount = payment.TotalAmount;
            if (payment.InstallmentId.HasValue)
            {
                var installment = await _context.Installments.FindAsync(payment.InstallmentId.Value);
                if (installment != null)
                {
                    if (installment.FineAmount > 0)
                    {
                        payment.FinePaid = Math.Min(remainingAmount, installment.FineAmount);
                        remainingAmount -= payment.FinePaid;
                    }
                    decimal interestDue = installment.InterestAmount - (installment.PaidAmount > 0 
                        ? installment.PaidAmount * (installment.InterestAmount / installment.TotalAmount) 
                        : 0);
                    if (remainingAmount > 0 && interestDue > 0)
                    {
                        payment.InterestPaid = Math.Min(remainingAmount, interestDue);
                        remainingAmount -= payment.InterestPaid;
                    }
                    decimal principalDue = installment.PrincipalAmount - (installment.PaidAmount > 0 
                        ? installment.PaidAmount * (installment.PrincipalAmount / installment.TotalAmount) 
                        : 0);
                    if (remainingAmount > 0 && principalDue > 0)
                    {
                        payment.PrincipalPaid = Math.Min(remainingAmount, principalDue);
                        remainingAmount -= payment.PrincipalPaid;
                    }
                    installment.PaidAmount += payment.TotalAmount - payment.FinePaid;
                    installment.RemainingAmount = installment.TotalAmount - installment.PaidAmount;
                    if (installment.RemainingAmount <= 0)
                    {
                        installment.Status = InstallmentStatus.Paid;
                        installment.PaymentDate = DateTime.UtcNow;
                    }
                    else
                    {
                        installment.Status = InstallmentStatus.Partial;
                    }
                }
            }
            loan.PaidAmount += payment.PrincipalPaid + payment.InterestPaid;
            loan.RemainingBalance = loan.TotalPayable - loan.PaidAmount;
            loan.LastPaymentDate = DateTime.UtcNow;
            loan.PaidInstallments = loan.Installments.Count(i => i.Status == InstallmentStatus.Paid);
            if (loan.RemainingBalance <= 0)
            {
                loan.Status = LoanStatus.Closed;
                loan.ClosedDate = DateTime.UtcNow;
            }
            else
            {
                loan.Status = LoanStatus.Active;
            }
            payment.PaymentCode = await GeneratePaymentCodeAsync();
            payment.PaymentDate = DateTime.UtcNow;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _accountingService.RecordLoanRepaymentAsync(payment, payment.CollectedBy);
            await transaction.CommitAsync();
            return payment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<List<Payment>> GetPaymentsByLoanIdAsync(int loanId)
    {
        return await _context.Payments
            .Where(p => p.LoanId == loanId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
    public async Task<List<Payment>> GetPaymentsByMemberIdAsync(int memberId)
    {
        return await _context.Payments
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
    public async Task<decimal> CalculateFineAsync(int installmentId)
    {
        var installment = await _context.Installments.FindAsync(installmentId);
        if (installment == null)
            return 0;
        var lateDays = (DateTime.UtcNow - installment.DueDate).TotalDays;
        if (lateDays <= 0)
            return 0;
        const decimal finePerDay = 5;
        return (decimal)lateDays * finePerDay;
    }
    private async Task<string> GeneratePaymentCodeAsync()
    {
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.Payments.CountAsync() + 1;
        return $"PAY-{year}-{count:D6}";
    }
}
