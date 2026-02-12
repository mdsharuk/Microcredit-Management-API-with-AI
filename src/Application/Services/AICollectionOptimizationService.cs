using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AICollectionOptimizationService : IAICollectionOptimizationService
{
    private readonly MicrocreditDbContext _context;
    public AICollectionOptimizationService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task<CollectionOptimizationResponse> GetCollectionInsightsAsync(CollectionOptimizationRequest request)
    {
        var branchInsights = await GetBranchInsightsAsync(request.BranchId);
        var officerInsights = await GetOfficerInsightsAsync(request.OfficerId);
        var highRiskAreas = await GetHighRiskAreasAsync();
        var allLoans = await _context.Loans
            .Where(l => l.Status != Domain.Enums.LoanStatus.Pending)
            .ToListAsync();
        var totalDisbursed = allLoans.Sum(l => l.TotalPayable);
        var totalRecovered = allLoans.Sum(l => l.PaidAmount);
        var overallRecoveryRate = totalDisbursed > 0 ? (totalRecovered / totalDisbursed) * 100 : 0;
        var predictedCollection = await PredictNextMonthCollectionAsync(request.BranchId);
        var recommendations = GenerateRecommendations(branchInsights, officerInsights, highRiskAreas);
        return new CollectionOptimizationResponse
        {
            BranchInsights = branchInsights,
            OfficerInsights = officerInsights,
            HighRiskAreas = highRiskAreas,
            Recommendations = recommendations,
            OverallRecoveryRate = Math.Round(overallRecoveryRate, 2),
            PredictedNextMonthRecovery = Math.Round(predictedCollection, 2)
        };
    }
    private async Task<List<BranchPerformanceInsight>> GetBranchInsightsAsync(int? branchId = null)
    {
        var branches = await _context.Branches
            .Where(b => !branchId.HasValue || b.Id == branchId.Value)
            .ToListAsync();
        var insights = new List<BranchPerformanceInsight>();
        foreach (var branch in branches)
        {
            var loans = await _context.Loans
                .Where(l => l.Member.BranchId == branch.Id)
                .ToListAsync();
            if (!loans.Any())
                continue;
            var totalDue = loans.Sum(l => l.TotalPayable);
            var totalPaid = loans.Sum(l => l.PaidAmount);
            var recoveryRate = totalDue > 0 ? (totalPaid / totalDue) * 100 : 0;
            var overdueLoans = loans.Where(l => 
                _context.Installments.Any(i => 
                    i.LoanId == l.Id && 
                    i.LateDays > 30 && 
                    i.Status != Domain.Enums.InstallmentStatus.Paid
                )).ToList();
            var parValue = overdueLoans.Sum(l => l.RemainingBalance);
            var totalOutstanding = loans.Where(l => l.Status == Domain.Enums.LoanStatus.Active)
                                       .Sum(l => l.RemainingBalance);
            var par = totalOutstanding > 0 ? (parValue / totalOutstanding) * 100 : 0;
            var issues = new List<string>();
            var strengths = new List<string>();
            if (recoveryRate < 70)
                issues.Add($"Low recovery rate: {recoveryRate:F1}%");
            else if (recoveryRate > 90)
                strengths.Add($"Excellent recovery rate: {recoveryRate:F1}%");
            if (par > 10)
                issues.Add($"High PAR: {par:F1}%");
            else if (par < 5)
                strengths.Add($"Low PAR: {par:F1}%");
            string category;
            if (recoveryRate >= 90 && par < 5)
                category = "Excellent";
            else if (recoveryRate >= 75 && par < 10)
                category = "Good";
            else if (recoveryRate >= 60)
                category = "NeedsImprovement";
            else
                category = "Critical";
            insights.Add(new BranchPerformanceInsight
            {
                BranchId = branch.Id,
                BranchName = branch.BranchName,
                RecoveryRate = Math.Round(recoveryRate, 2),
                PortfolioAtRisk = Math.Round(par, 2),
                PerformanceCategory = category,
                Issues = issues,
                Strengths = strengths
            });
        }
        return insights.OrderBy(i => i.RecoveryRate).ToList();
    }
    private async Task<List<OfficerPerformanceInsight>> GetOfficerInsightsAsync(int? officerId = null)
    {
        await Task.CompletedTask;
        return new List<OfficerPerformanceInsight>();
    }
    public async Task<List<BranchPerformanceInsight>> GetUnderperformingBranchesAsync()
    {
        var allInsights = await GetBranchInsightsAsync();
        return allInsights
            .Where(i => i.PerformanceCategory == "NeedsImprovement" || 
                       i.PerformanceCategory == "Critical")
            .ToList();
    }
    public async Task<List<OfficerPerformanceInsight>> GetUnderperformingOfficersAsync()
    {
        var allInsights = await GetOfficerInsightsAsync();
        return allInsights
            .Where(i => i.PerformanceRating == "Average" || 
                       i.PerformanceRating == "Poor")
            .ToList();
    }
    public async Task<List<HighRiskArea>> GetHighRiskAreasAsync()
    {
        var members = await _context.Members
            .Include(m => m.Loans)
            .ToListAsync();
        // Group by village
        var areaGroups = members
            .GroupBy(m => new { m.Village, m.Address })
            .Select(g => new
            {
                g.Key.Village,
                g.Key.Address,
                Members = g.ToList()
            })
            .ToList();
        var highRiskAreas = new List<HighRiskArea>();
        foreach (var area in areaGroups)
        {
            if (area.Members.Count < 3)
                continue;
            var defaultedCount = area.Members.Count(m => 
                m.Loans.Any(l => l.Status == Domain.Enums.LoanStatus.WrittenOff));
            var defaultRate = (decimal)defaultedCount / area.Members.Count * 100;
            if (defaultRate > 20)
            {
                string riskLevel;
                string reason;
                if (defaultRate > 50)
                {
                    riskLevel = "Critical";
                    reason = "More than 50% members have defaulted";
                }
                else if (defaultRate > 35)
                {
                    riskLevel = "High";
                    reason = "High concentration of defaults";
                }
                else
                {
                    riskLevel = "Medium";
                    reason = "Elevated default rate";
                }
                highRiskAreas.Add(new HighRiskArea
                {
                    AreaName = area.Address,
                    Village = area.Village,
                    TotalMembers = area.Members.Count,
                    DefaultedMembers = defaultedCount,
                    DefaultRate = Math.Round(defaultRate, 2),
                    RiskLevel = riskLevel,
                    Reason = reason
                });
            }
        }
        return highRiskAreas.OrderByDescending(a => a.DefaultRate).ToList();
    }
    public async Task<decimal> PredictNextMonthCollectionAsync(int? branchId = null)
    {
        var upcomingInstallments = await _context.Installments
            .Include(i => i.Loan)
                .ThenInclude(l => l.Member)
            .Where(i => i.DueDate >= DateTime.UtcNow &&
                       i.DueDate <= DateTime.UtcNow.AddDays(30) &&
                       i.Status == Domain.Enums.InstallmentStatus.Pending &&
                       (!branchId.HasValue || i.Loan.Member.BranchId == branchId.Value))
            .ToListAsync();
        var expectedAmount = upcomingInstallments.Sum(i => i.TotalAmount);
        var historicalRate = await CalculateHistoricalCollectionRateAsync();
        var predictedCollection = expectedAmount * Math.Min(historicalRate, 0.95m);
        return predictedCollection;
    }
    private async Task<decimal> CalculateHistoricalCollectionRateAsync()
    {
        var lastMonthInstallments = await _context.Installments
            .Where(i => i.DueDate >= DateTime.UtcNow.AddDays(-30) &&
                       i.DueDate < DateTime.UtcNow)
            .ToListAsync();
        if (!lastMonthInstallments.Any())
            return 0.85m;
        var totalDue = lastMonthInstallments.Sum(i => i.TotalAmount);
        var totalPaid = lastMonthInstallments
            .Where(i => i.Status == Domain.Enums.InstallmentStatus.Paid)
            .Sum(i => i.PaidAmount);
        return totalDue > 0 ? totalPaid / totalDue : 0.85m;
    }
    private List<string> GenerateRecommendations(
        List<BranchPerformanceInsight> branches,
        List<OfficerPerformanceInsight> officers,
        List<HighRiskArea> areas)
    {
        var recommendations = new List<string>();
        var criticalBranches = branches.Where(b => b.PerformanceCategory == "Critical").ToList();
        if (criticalBranches.Any())
        {
            recommendations.Add($"⚠️  {criticalBranches.Count} branch(es) need immediate intervention");
        }
        var poorOfficers = officers.Where(o => o.PerformanceRating == "Poor").ToList();
        if (poorOfficers.Any())
        {
            recommendations.Add($"👮 {poorOfficers.Count} field officer(s) require training and support");
        }
        var criticalAreas = areas.Where(a => a.RiskLevel == "Critical").ToList();
        if (criticalAreas.Any())
        {
            recommendations.Add($"📍 Avoid new lending in {criticalAreas.Count} high-risk area(s)");
        }
        var avgRecovery = branches.Any() ? branches.Average(b => b.RecoveryRate) : 0;
        if (avgRecovery < 80)
        {
            recommendations.Add("📊 Implement stricter follow-up procedures");
            recommendations.Add("💬 Consider SMS reminder system");
        }
        var avgPAR = branches.Any() ? branches.Average(b => b.PortfolioAtRisk) : 0;
        if (avgPAR > 10)
        {
            recommendations.Add("🎯 Focus on reducing Portfolio at Risk (PAR)");
        }
        if (!recommendations.Any())
        {
            recommendations.Add("✅ Overall performance is good. Maintain current standards.");
        }
        return recommendations;
    }
}
