using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
public static class SeedData
{
    public static async Task InitializeAsync(MicrocreditDbContext context, IConfiguration configuration)
    {
        await context.Database.MigrateAsync();
        if (!await context.Users.AnyAsync())
        {
            var adminConfig = configuration.GetSection("DefaultAdmin");
            var adminUser = new User
            {
                Username = adminConfig["Username"] ?? "admin",
                Email = adminConfig["Email"] ?? "admin@microcredit.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminConfig["Password"] ?? "Admin@123"),
                FullName = adminConfig["FullName"] ?? "System Administrator",
                Phone = "0000000000",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
        if (!await context.LedgerAccounts.AnyAsync())
        {
            var ledgerAccounts = new List<LedgerAccount>
            {
                new LedgerAccount
                {
                    AccountCode = "CASH",
                    AccountName = "Cash",
                    AccountType = AccountType.Asset,
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LedgerAccount
                {
                    AccountCode = "LOAN_RECEIVABLE",
                    AccountName = "Loan Receivable",
                    AccountType = AccountType.Asset,
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LedgerAccount
                {
                    AccountCode = "SAVINGS_LIABILITY",
                    AccountName = "Savings Liability",
                    AccountType = AccountType.Liability,
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LedgerAccount
                {
                    AccountCode = "INTEREST_INCOME",
                    AccountName = "Interest Income",
                    AccountType = AccountType.Income,
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new LedgerAccount
                {
                    AccountCode = "FINE_INCOME",
                    AccountName = "Fine Income",
                    AccountType = AccountType.Income,
                    Balance = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.LedgerAccounts.AddRange(ledgerAccounts);
            await context.SaveChangesAsync();
        }
    }
}
