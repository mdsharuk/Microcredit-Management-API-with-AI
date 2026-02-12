using Domain.Enums;
namespace Domain.Entities;
public class LedgerAccount : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    // Navigation
    public LedgerAccount? ParentAccount { get; set; }
    public ICollection<LedgerAccount> ChildAccounts { get; set; } = new List<LedgerAccount>();
    public ICollection<Transaction> DebitTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> CreditTransactions { get; set; } = new List<Transaction>();
}
