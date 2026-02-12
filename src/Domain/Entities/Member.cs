using Domain.Enums;
namespace Domain.Entities;
public class Member : BaseEntity
{
    public string MemberCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string SpouseName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string PostOffice { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    // Nominee Information
    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string NomineePhone { get; set; } = string.Empty;
    public string NomineeNID { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int GroupId { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public DateTime JoinDate { get; set; }
    public string? PhotoPath { get; set; }
    public string? NIDCopyPath { get; set; }
    public int LoanCycle { get; set; } = 0;
    // Navigation
    public Branch Branch { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public SavingsAccount? SavingsAccount { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
