using Domain.Entities;
using Domain.Enums;
namespace Application.DTOs;
public class LoanApplicationDto
{
    public Guid MemberId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public InterestType InterestType { get; set; }
    public int DurationInWeeks { get; set; }
    public string Purpose { get; set; } = string.Empty;
}
public class LoanApprovalDto
{
    public Guid LoanId { get; set; }
    public Guid ApprovedBy { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}
public class PaymentDto
{
    public Guid LoanId { get; set; }
    public Guid MemberId { get; set; }
    public Guid? InstallmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public Guid CollectedBy { get; set; }
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
    public Guid BranchId { get; set; }
    public Guid GroupId { get; set; }
    // Nominee
    public string NomineeName { get; set; } = string.Empty;
    public string NomineeRelation { get; set; } = string.Empty;
    public string NomineePhone { get; set; } = string.Empty;
    public string NomineeNID { get; set; } = string.Empty;
}
public class GroupCreationDto
{
    public string GroupName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public Guid FieldOfficerId { get; set; }
    public DayOfWeek MeetingDay { get; set; }
    public TimeSpan MeetingTime { get; set; }
}
public class SavingsDepositDto
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public Guid ProcessedBy { get; set; }
    public string? Remarks { get; set; }
}
public class SavingsWithdrawalDto
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public Guid ProcessedBy { get; set; }
    public string? Remarks { get; set; }
}
