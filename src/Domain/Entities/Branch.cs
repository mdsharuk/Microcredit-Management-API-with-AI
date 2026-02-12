namespace Domain.Entities;
public class Branch : BaseEntity
{
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
    // Navigation
    public User? Manager { get; set; }
    public ICollection<User> Staff { get; set; } = new List<User>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
