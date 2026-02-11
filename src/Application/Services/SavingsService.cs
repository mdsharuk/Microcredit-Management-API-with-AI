using Domain.Entities;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class SavingsService : ISavingsService
{
    private readonly MicrocreditDbContext _context;
    private readonly IAccountingService _accountingService;
    public SavingsService(MicrocreditDbContext context, IAccountingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }
    public async Task<SavingsAccount> CreateSavingsAccountAsync(SavingsAccount account)
    {
        var existing = await _context.SavingsAccounts
            .FirstOrDefaultAsync(s => s.MemberId == account.MemberId);
        if (existing != null)
            throw new Exception("Member already has a savings account");
        account.AccountNumber = await GenerateAccountNumberAsync();
        account.OpeningDate = DateTime.UtcNow;
        account.Balance = 0;
        account.TotalDeposits = 0;
        account.TotalWithdrawals = 0;
        account.IsActive = true;
        _context.SavingsAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }
    public async Task<SavingsTransaction> DepositAsync(Guid accountId, decimal amount, Guid processedBy, string? remarks = null)
    {
        if (amount <= 0)
            throw new Exception("Deposit amount must be greater than zero");
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _context.SavingsAccounts.FindAsync(accountId);
            if (account == null)
                throw new Exception("Savings account not found");
            if (!account.IsActive)
                throw new Exception("Savings account is not active");
            account.Balance += amount;
            account.TotalDeposits += amount;
            var savingsTransaction = new SavingsTransaction
            {
                Id = Guid.NewGuid(),
                SavingsAccountId = accountId,
                TransactionType = "Deposit",
                Amount = amount,
                BalanceAfter = account.Balance,
                TransactionDate = DateTime.UtcNow,
                ProcessedBy = processedBy,
                Remarks = remarks,
                CreatedAt = DateTime.UtcNow
            };
            _context.SavingsTransactions.Add(savingsTransaction);
            await _context.SaveChangesAsync();
            // Record in accounting
            await _accountingService.RecordSavingsTransactionAsync(savingsTransaction, processedBy);
            await transaction.CommitAsync();
            return savingsTransaction;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<SavingsTransaction> WithdrawAsync(Guid accountId, decimal amount, Guid processedBy, string? remarks = null)
    {
        if (amount <= 0)
            throw new Exception("Withdrawal amount must be greater than zero");
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _context.SavingsAccounts.FindAsync(accountId);
            if (account == null)
                throw new Exception("Savings account not found");
            if (!account.IsActive)
                throw new Exception("Savings account is not active");
            if (account.Balance < amount)
                throw new Exception("Insufficient balance");
            account.Balance -= amount;
            account.TotalWithdrawals += amount;
            var savingsTransaction = new SavingsTransaction
            {
                Id = Guid.NewGuid(),
                SavingsAccountId = accountId,
                TransactionType = "Withdrawal",
                Amount = amount,
                BalanceAfter = account.Balance,
                TransactionDate = DateTime.UtcNow,
                ProcessedBy = processedBy,
                Remarks = remarks,
                CreatedAt = DateTime.UtcNow
            };
            _context.SavingsTransactions.Add(savingsTransaction);
            await _context.SaveChangesAsync();
            // Record in accounting
            await _accountingService.RecordSavingsTransactionAsync(savingsTransaction, processedBy);
            await transaction.CommitAsync();
            return savingsTransaction;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<SavingsAccount?> GetAccountByMemberIdAsync(Guid memberId)
    {
        return await _context.SavingsAccounts
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.MemberId == memberId);
    }
    public async Task<decimal> GetBalanceAsync(Guid accountId)
    {
        var account = await _context.SavingsAccounts.FindAsync(accountId);
        return account?.Balance ?? 0;
    }
    private async Task<string> GenerateAccountNumberAsync()
    {
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.SavingsAccounts.CountAsync() + 1;
        return $"SAV-{year}-{count:D6}";
    }
}
