using Domain.Enums;
namespace Domain.Entities;
public class Group : BaseEntity
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public int? FieldOfficerId { get; set; }
    public int? LeaderId { get; set; }
    public DayOfWeek MeetingDay { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public GroupStatus Status { get; set; } = GroupStatus.Forming;
    public int MinMembers { get; set; } = 5;
    public DateTime? ActivationDate { get; set; }
    public decimal PerformanceRating { get; set; }
    // Navigation
    public Branch Branch { get; set; } = null!;
    public User? FieldOfficer { get; set; }
    public Member? Leader { get; set; }
    public ICollection<Member> Members { get; set; } = new List<Member>();
}
