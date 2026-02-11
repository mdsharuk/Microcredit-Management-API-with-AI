using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AIFraudDetectionService : IAIFraudDetectionService
{
    private readonly MicrocreditDbContext _context;
    public AIFraudDetectionService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task<FraudDetectionResponse> DetectFraudAsync(FraudDetectionRequest request)
    {
        var alerts = new List<FraudAlert>();
        var fraudScore = 0m;
        if (!string.IsNullOrEmpty(request.NID))
        {
            var duplicateNID = await CheckDuplicateNIDAsync(request.NID);
            if (duplicateNID)
            {
                alerts.Add(new FraudAlert
                {
                    Type = "DuplicateNID",
                    Severity = "Critical",
                    Description = "NID already exists in database",
                    Evidence = $"NID {request.NID} is already registered"
                });
                fraudScore += 40;
            }
        }
        if (!string.IsNullOrEmpty(request.Phone))
        {
            var validPhone = await ValidatePhonePatternAsync(request.Phone);
            if (!validPhone)
            {
                alerts.Add(new FraudAlert
                {
                    Type = "InvalidPhone",
                    Severity = "Medium",
                    Description = "Phone number format is suspicious",
                    Evidence = $"Phone {request.Phone} doesn't match expected pattern"
                });
                fraudScore += 15;
            }
            var phoneCount = await _context.Members
                .CountAsync(m => m.Phone == request.Phone && m.Id != request.MemberId);
            if (phoneCount > 2)
            {
                alerts.Add(new FraudAlert
                {
                    Type = "DuplicatePhone",
                    Severity = "High",
                    Description = "Same phone used by multiple members",
                    Evidence = $"Phone {request.Phone} used by {phoneCount} different members"
                });
                fraudScore += 25;
            }
        }
        if (!string.IsNullOrEmpty(request.Address))
        {
            var similarAddresses = await _context.Members
                .Where(m => m.Address.Contains(request.Address.Substring(0, Math.Min(10, request.Address.Length))))
                .CountAsync();
            if (similarAddresses > 10)
            {
                alerts.Add(new FraudAlert
                {
                    Type = "SuspiciousAddress",
                    Severity = "Medium",
                    Description = "Too many members with similar address",
                    Evidence = $"{similarAddresses} members share similar address pattern"
                });
                fraudScore += 15;
            }
        }
        if (request.GroupId.HasValue)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId.Value);
            if (group != null)
            {
                var groupAge = (DateTime.UtcNow - group.CreatedAt).TotalDays;
                if (groupAge < 1 && group.Members.Count >= 5)
                {
                    alerts.Add(new FraudAlert
                    {
                        Type = "RapidGroupFormation",
                        Severity = "High",
                        Description = "Group formed and filled too quickly",
                        Evidence = $"Group filled with {group.Members.Count} members in less than 24 hours"
                    });
                    fraudScore += 20;
                }
            }
        }
        if (request.MemberId.HasValue)
        {
            var member = await _context.Members
                .Include(m => m.Loans)
                .FirstOrDefaultAsync(m => m.Id == request.MemberId.Value);
            if (member != null)
            {
                var memberAge = (DateTime.UtcNow - member.CreatedAt).TotalDays;
                var hasLargeLoanRequest = member.Loans.Any(l => l.LoanAmount > 50000);
                if (memberAge < 7 && hasLargeLoanRequest)
                {
                    alerts.Add(new FraudAlert
                    {
                        Type = "NewMemberHighLoan",
                        Severity = "High",
                        Description = "New member requesting unusually large loan",
                        Evidence = $"Member created {memberAge:F0} days ago requesting high-value loan"
                    });
                    fraudScore += 25;
                }
            }
        }
        fraudScore = Math.Min(100, fraudScore);
        string recommendation;
        if (fraudScore < 20)
        {
            recommendation = "APPROVE - No significant fraud indicators detected.";
        }
        else if (fraudScore < 50)
        {
            recommendation = "REVIEW - Some suspicious patterns detected. Verify documents carefully.";
        }
        else if (fraudScore < 75)
        {
            recommendation = "INVESTIGATE - Multiple fraud indicators. Conduct thorough investigation.";
        }
        else
        {
            recommendation = "REJECT - Critical fraud indicators detected. Do not proceed.";
        }
        return new FraudDetectionResponse
        {
            IsSuspicious = fraudScore >= 30,
            FraudScore = Math.Round(fraudScore, 2),
            Alerts = alerts,
            RecommendedAction = recommendation
        };
    }
    public async Task<bool> CheckDuplicateNIDAsync(string nid)
    {
        return await _context.Members.AnyAsync(m => m.NID == nid);
    }
    public async Task<bool> ValidatePhonePatternAsync(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return false;
        var cleanPhone = phone.Replace("-", "").Replace(" ", "");
        if (cleanPhone.Length != 11 || !cleanPhone.StartsWith("01"))
            return false;
        return await Task.FromResult(true);
    }
    public async Task<FraudDetectionResponse> DetectSuspiciousGroupAsync(Guid groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
            throw new Exception("Group not found");
        var alerts = new List<FraudAlert>();
        var fraudScore = 0m;
        var joinDates = group.Members.Select(m => m.JoinDate.Date).Distinct().ToList();
        if (joinDates.Count == 1 && group.Members.Count >= 5)
        {
            alerts.Add(new FraudAlert
            {
                Type = "SameJoinDate",
                Severity = "High",
                Description = "All members joined on same date",
                Evidence = $"All {group.Members.Count} members registered on {joinDates[0]:yyyy-MM-dd}"
            });
            fraudScore += 30;
        }
        var nids = group.Members.Select(m => m.NID).ToList();
        var similarNIDCount = 0;
        for (int i = 0; i < nids.Count - 1; i++)
        {
            for (int j = i + 1; j < nids.Count; j++)
            {
                if (nids[i].Substring(0, 8) == nids[j].Substring(0, 8))
                {
                    similarNIDCount++;
                }
            }
        }
        if (similarNIDCount > 0)
        {
            alerts.Add(new FraudAlert
            {
                Type = "SimilarNIDs",
                Severity = "Critical",
                Description = "Multiple members have suspiciously similar NIDs",
                Evidence = $"{similarNIDCount} pairs of similar NIDs detected"
            });
            fraudScore += 40;
        }
        var addresses = group.Members.Select(m => m.Address).Distinct().ToList();
        if (addresses.Count == 1)
        {
            alerts.Add(new FraudAlert
            {
                Type = "SameAddress",
                Severity = "Medium",
                Description = "All members share same address",
                Evidence = $"All members registered with address: {addresses[0]}"
            });
            fraudScore += 20;
        }
        fraudScore = Math.Min(100, fraudScore);
        return new FraudDetectionResponse
        {
            IsSuspicious = fraudScore >= 30,
            FraudScore = Math.Round(fraudScore, 2),
            Alerts = alerts,
            RecommendedAction = fraudScore >= 50 
                ? "INVESTIGATE - Suspicious group patterns detected" 
                : "MONITOR - Some unusual patterns, monitor closely"
        };
    }
}
