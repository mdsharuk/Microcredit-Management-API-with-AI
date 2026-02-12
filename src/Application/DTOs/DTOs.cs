using Domain.Entities;
using Domain.Enums;
namespace Application.DTOs;
public class LoanApplicationDto
{
    public int MemberId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public InterestType InterestType { get; set; }
    public int DurationInWeeks { get; set; }
    public string Purpose { get; set; } = string.Empty;
}
public class LoanApprovalDto
{
    public int LoanId { get; set; }
    public int ApprovedBy { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}
public class PaymentDto
{
    public int LoanId { get; set; }
    public int MemberId { get; set; }
    public int? InstallmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public int CollectedBy { get; set; }
    public decimal? SavingsAmount { get; set; }
    public string? Remarks { get; set; }
}
public class MemberRegistrationDto
{
    public string FullName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string SpouseName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int GroupId { get; set; }
    // Nominee
    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string NomineePhone { get; set; } = string.Empty;
    public string NomineeNID { get; set; } = string.Empty;
}
public class GroupCreationDto
{
    public string GroupName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int FieldOfficerId { get; set; }
    public DayOfWeek MeetingDay { get; set; }
    public TimeSpan MeetingTime { get; set; }
}
public class SavingsDepositDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public int ProcessedBy { get; set; }
    public string? Remarks { get; set; }
}
public class SavingsWithdrawalDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public int ProcessedBy { get; set; }
    public string? Remarks { get; set; }
}
