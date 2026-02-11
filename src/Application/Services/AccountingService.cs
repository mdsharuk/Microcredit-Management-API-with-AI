using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AccountingService : IAccountingService
{
    private readonly MicrocreditDbContext _context;
    private Dictionary<string, Guid> _accountCache = new();
    public AccountingService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task RecordLoanDisbursementAsync(Loan loan, Guid userId)
    {
        var loanReceivableAccount = await GetOrCreateLedgerAccountAsync("LOAN_RECEIVABLE", "Loan Receivable", AccountType.Asset);
        var cashAccount = await GetOrCreateLedgerAccountAsync("CASH", "Cash", AccountType.Asset);
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionCode = await GenerateTransactionCodeAsync(),
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.LoanDisbursement,
            DebitAccountId = loanReceivableAccount.Id,
            CreditAccountId = cashAccount.Id,
            Amount = loan.LoanAmount,
            Description = $"Loan disbursement for {loan.LoanCode}",
            Reference = loan.LoanCode,
            LoanId = loan.Id,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        loanReceivableAccount.Balance += loan.LoanAmount;
        cashAccount.Balance -= loan.LoanAmount;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
    public async Task RecordLoanRepaymentAsync(Payment payment, Guid userId)
    {
        var cashAccount = await GetOrCreateLedgerAccountAsync("CASH", "Cash", AccountType.Asset);
        var loanReceivableAccount = await GetOrCreateLedgerAccountAsync("LOAN_RECEIVABLE", "Loan Receivable", AccountType.Asset);
        var interestIncomeAccount = await GetOrCreateLedgerAccountAsync("INTEREST_INCOME", "Interest Income", AccountType.Income);
        if (payment.PrincipalPaid > 0)
        {
            var principalTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = await GenerateTransactionCodeAsync(),
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.LoanRepayment,
                DebitAccountId = cashAccount.Id,
                CreditAccountId = loanReceivableAccount.Id,
                Amount = payment.PrincipalPaid,
                Description = $"Loan principal payment for {payment.PaymentCode}",
                Reference = payment.PaymentCode,
                LoanId = payment.LoanId,
                PaymentId = payment.Id,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            cashAccount.Balance += payment.PrincipalPaid;
            loanReceivableAccount.Balance -= payment.PrincipalPaid;
            _context.Transactions.Add(principalTransaction);
        }
        if (payment.InterestPaid > 0)
        {
            var interestTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = await GenerateTransactionCodeAsync(),
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.InterestIncome,
                DebitAccountId = cashAccount.Id,
                CreditAccountId = interestIncomeAccount.Id,
                Amount = payment.InterestPaid,
                Description = $"Interest income from {payment.PaymentCode}",
                Reference = payment.PaymentCode,
                LoanId = payment.LoanId,
                PaymentId = payment.Id,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            cashAccount.Balance += payment.InterestPaid;
            interestIncomeAccount.Balance += payment.InterestPaid;
            _context.Transactions.Add(interestTransaction);
        }
        if (payment.FinePaid > 0)
        {
            var fineIncomeAccount = await GetOrCreateLedgerAccountAsync("FINE_INCOME", "Fine Income", AccountType.Income);
            var fineTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = await GenerateTransactionCodeAsync(),
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.FineCollection,
                DebitAccountId = cashAccount.Id,
                CreditAccountId = fineIncomeAccount.Id,
                Amount = payment.FinePaid,
                Description = $"Fine collection from {payment.PaymentCode}",
                Reference = payment.PaymentCode,
                LoanId = payment.LoanId,
                PaymentId = payment.Id,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            cashAccount.Balance += payment.FinePaid;
            fineIncomeAccount.Balance += payment.FinePaid;
            _context.Transactions.Add(fineTransaction);
        }
        await _context.SaveChangesAsync();
    }
    public async Task RecordSavingsTransactionAsync(SavingsTransaction savingsTransaction, Guid userId)
    {
        var cashAccount = await GetOrCreateLedgerAccountAsync("CASH", "Cash", AccountType.Asset);
        var savingsLiabilityAccount = await GetOrCreateLedgerAccountAsync("SAVINGS_LIABILITY", "Savings Liability", AccountType.Liability);
        Transaction transaction;
        if (savingsTransaction.TransactionType == "Deposit")
        {
            transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = await GenerateTransactionCodeAsync(),
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.SavingsDeposit,
                DebitAccountId = cashAccount.Id,
                CreditAccountId = savingsLiabilityAccount.Id,
                Amount = savingsTransaction.Amount,
                Description = $"Savings deposit",
                Reference = savingsTransaction.Reference,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            cashAccount.Balance += savingsTransaction.Amount;
            savingsLiabilityAccount.Balance += savingsTransaction.Amount;
        }
        else
        {
            transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = await GenerateTransactionCodeAsync(),
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.SavingsWithdrawal,
                DebitAccountId = savingsLiabilityAccount.Id,
                CreditAccountId = cashAccount.Id,
                Amount = savingsTransaction.Amount,
                Description = $"Savings withdrawal",
                Reference = savingsTransaction.Reference,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            savingsLiabilityAccount.Balance -= savingsTransaction.Amount;
            cashAccount.Balance -= savingsTransaction.Amount;
        }
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
    private async Task<LedgerAccount> GetOrCreateLedgerAccountAsync(string accountCode, string accountName, AccountType accountType)
    {
        if (_accountCache.ContainsKey(accountCode))
        {
            var cachedAccount = await _context.LedgerAccounts.FindAsync(_accountCache[accountCode]);
            if (cachedAccount != null)
                return cachedAccount;
        }
        var account = await _context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode);
        if (account == null)
        {
            account = new LedgerAccount
            {
                Id = Guid.NewGuid(),
                AccountCode = accountCode,
                AccountName = accountName,
                AccountType = accountType,
                Balance = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.LedgerAccounts.Add(account);
            await _context.SaveChangesAsync();
        }
        _accountCache[accountCode] = account.Id;
        return account;
    }
    private async Task<string> GenerateTransactionCodeAsync()
    {
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.Transactions.CountAsync() + 1;
        return $"TXN-{year}-{count:D7}";
    }
}
