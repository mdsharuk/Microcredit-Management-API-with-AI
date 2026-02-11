using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(15);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasOne(u => u.Branch)
            .WithMany(b => b.Staff)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BranchCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.BranchName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Phone).HasMaxLength(15);
        builder.HasIndex(b => b.BranchCode).IsUnique();
    }
}
public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.GroupCode).IsRequired().HasMaxLength(20);
        builder.Property(g => g.GroupName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.PerformanceRating).HasColumnType("decimal(3,2)");
        builder.HasIndex(g => g.GroupCode).IsUnique();
        builder.HasOne(g => g.Branch)
            .WithMany(b => b.Groups)
            .HasForeignKey(g => g.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(g => g.FieldOfficer)
            .WithMany()
            .HasForeignKey(g => g.FieldOfficerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(g => g.Leader)
            .WithMany()
            .HasForeignKey(g => g.LeaderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MemberCode).IsRequired().HasMaxLength(20);
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.NID).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Phone).HasMaxLength(15);
        builder.Property(m => m.Address).HasMaxLength(500);
        builder.HasIndex(m => m.MemberCode).IsUnique();
        builder.HasIndex(m => m.NID).IsUnique();
        builder.HasOne(m => m.Branch)
            .WithMany(b => b.Members)
            .HasForeignKey(m => m.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.SavingsAccount)
            .WithOne(s => s.Member)
            .HasForeignKey<SavingsAccount>(s => s.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LoanCode).IsRequired().HasMaxLength(20);
        builder.Property(l => l.LoanAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.InterestRate).HasColumnType("decimal(5,2)");
        builder.Property(l => l.TotalInterest).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalPayable).HasColumnType("decimal(18,2)");
        builder.Property(l => l.WeeklyInstallment).HasColumnType("decimal(18,2)");
        builder.Property(l => l.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.RemainingBalance).HasColumnType("decimal(18,2)");
        builder.HasIndex(l => l.LoanCode).IsUnique();
        builder.HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Branch)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.Group)
            .WithMany()
            .HasForeignKey(l => l.GroupId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(l => l.Approver)
            .WithMany()
            .HasForeignKey(l => l.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
public class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.PrincipalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.InterestAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.RemainingAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.FineAmount).HasColumnType("decimal(18,2)");
        builder.HasOne(i => i.Loan)
            .WithMany(l => l.Installments)
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PaymentCode).IsRequired().HasMaxLength(20);
        builder.Property(p => p.PrincipalPaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.InterestPaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.FinePaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(p => p.PaymentCode).IsUnique();
        builder.HasOne(p => p.Loan)
            .WithMany(l => l.Payments)
            .HasForeignKey(p => p.LoanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Member)
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Installment)
            .WithMany()
            .HasForeignKey(p => p.InstallmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.Collector)
            .WithMany()
            .HasForeignKey(p => p.CollectedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
{
    public void Configure(EntityTypeBuilder<SavingsAccount> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.AccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Balance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CompulsoryWeeklySavings).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalDeposits).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalWithdrawals).HasColumnType("decimal(18,2)");
        builder.HasIndex(s => s.AccountNumber).IsUnique();
    }
}
public class SavingsTransactionConfiguration : IEntityTypeConfiguration<SavingsTransaction>
{
    public void Configure(EntityTypeBuilder<SavingsTransaction> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Amount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.BalanceAfter).HasColumnType("decimal(18,2)");
        builder.HasOne(s => s.SavingsAccount)
            .WithMany(a => a.Transactions)
            .HasForeignKey(s => s.SavingsAccountId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Processor)
            .WithMany()
            .HasForeignKey(s => s.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class FineConfiguration : IEntityTypeConfiguration<Fine>
{
    public void Configure(EntityTypeBuilder<Fine> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FinePerDay).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TotalFine).HasColumnType("decimal(18,2)");
        builder.Property(f => f.PaidAmount).HasColumnType("decimal(18,2)");
        builder.HasOne(f => f.Installment)
            .WithMany()
            .HasForeignKey(f => f.InstallmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Loan)
            .WithMany()
            .HasForeignKey(f => f.LoanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(f => f.WaivedByUser)
            .WithMany()
            .HasForeignKey(f => f.WaivedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
public class LedgerAccountConfiguration : IEntityTypeConfiguration<LedgerAccount>
{
    public void Configure(EntityTypeBuilder<LedgerAccount> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.AccountCode).IsRequired().HasMaxLength(20);
        builder.Property(l => l.AccountName).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Balance).HasColumnType("decimal(18,2)");
        builder.HasIndex(l => l.AccountCode).IsUnique();
        builder.HasOne(l => l.ParentAccount)
            .WithMany(p => p.ChildAccounts)
            .HasForeignKey(l => l.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TransactionCode).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(t => t.TransactionCode).IsUnique();
        builder.HasOne(t => t.DebitAccount)
            .WithMany(a => a.DebitTransactions)
            .HasForeignKey(t => t.DebitAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.CreditAccount)
            .WithMany(a => a.CreditTransactions)
            .HasForeignKey(t => t.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Loan)
            .WithMany()
            .HasForeignKey(t => t.LoanId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.Payment)
            .WithMany()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
