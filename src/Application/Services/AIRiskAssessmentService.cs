using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AIRiskAssessmentService : IAIRiskAssessmentService
{
    private readonly MicrocreditDbContext _context;
    public AIRiskAssessmentService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task<RiskAssessmentResponse> AssessRiskAsync(RiskAssessmentRequest request)
    {
        var member = await _context.Members
            .Include(m => m.SavingsAccount)
            .Include(m => m.Loans)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId);
        if (member == null)
            throw new Exception("Member not found");
        var riskFactors = new List<string>();
        var positiveFactors = new List<string>();
        var riskScore = 50m;
        var savingsBalance = member.SavingsAccount?.Balance ?? 0;
        if (savingsBalance < 100)
        {
            riskFactors.Add("Low savings balance (below minimum)");
            riskScore += 15;
        }
        else if (savingsBalance > 1000)
        {
            positiveFactors.Add("Strong savings balance");
            riskScore -= 10;
        }
        var previousLoans = member.Loans.Where(l => l.Status == Domain.Enums.LoanStatus.Closed).ToList();
        var latePaymentCount = await _context.Payments
            .Join(_context.Installments,
                p => p.InstallmentId,
                i => i.Id,
                (p, i) => new { Payment = p, Installment = i })
            .Where(x => x.Installment.LateDays > 0 && 
                       _context.Loans.Any(l => l.Id == x.Payment.LoanId && l.MemberId == request.MemberId))
            .CountAsync();
        if (latePaymentCount > 5)
        {
            riskFactors.Add($"History of late payments ({latePaymentCount} times)");
            riskScore += 20;
        }
        else if (latePaymentCount == 0 && previousLoans.Any())
        {
            positiveFactors.Add("Perfect payment history");
            riskScore -= 15;
        }
        var loanToSavingsRatio = savingsBalance > 0 ? request.RequestedAmount / savingsBalance : 100;
        if (loanToSavingsRatio > 10)
        {
            riskFactors.Add("Loan amount too high compared to savings");
            riskScore += 15;
        }
        var hasActiveLoan = member.Loans.Any(l => 
            l.Status == Domain.Enums.LoanStatus.Active || 
            l.Status == Domain.Enums.LoanStatus.Disbursed);
        if (hasActiveLoan)
        {
            riskFactors.Add("Already has an active loan");
            riskScore += 25;
        }
        if (member.Group?.PerformanceRating < 0.5m)
        {
            riskFactors.Add("Group has poor performance rating");
            riskScore += 10;
        }
        else if (member.Group?.PerformanceRating > 0.8m)
        {
            positiveFactors.Add("Group has excellent performance");
            riskScore -= 10;
        }
        if (member.LoanCycle > 3)
        {
            positiveFactors.Add($"Experienced borrower (cycle {member.LoanCycle})");
            riskScore -= 5;
        }
        riskScore = Math.Max(0, Math.Min(100, riskScore));
        string riskCategory;
        bool isApproved;
        string recommendation;
        if (riskScore < 30)
        {
            riskCategory = "Low Risk";
            isApproved = true;
            recommendation = "APPROVE - Low risk borrower with strong indicators.";
        }
        else if (riskScore < 60)
        {
            riskCategory = "Medium Risk";
            isApproved = true;
            recommendation = "APPROVE WITH MONITORING - Acceptable risk, monitor payment behavior.";
        }
        else if (riskScore < 80)
        {
            riskCategory = "High Risk";
            isApproved = false;
            recommendation = "REVIEW REQUIRED - High risk factors detected. Manual review recommended.";
        }
        else
        {
            riskCategory = "Very High Risk";
            isApproved = false;
            recommendation = "REJECT - Too many risk factors. Consider after improving financial position.";
        }
        return new RiskAssessmentResponse
        {
            MemberId = member.Id,
            MemberName = member.FullName,
            RiskScore = Math.Round(riskScore, 2),
            RiskCategory = riskCategory,
            IsApprovalRecommended = isApproved,
            RiskFactors = riskFactors,
            PositiveFactors = positiveFactors,
            Recommendation = recommendation
        };
    }
    public async Task<BatchRiskAssessmentResponse> BatchAssessRiskAsync(BatchRiskAssessmentRequest request)
    {
        var assessments = new List<RiskAssessmentResponse>();
        foreach (var memberId in request.MemberIds)
        {
            try
            {
                var assessment = await AssessRiskAsync(new RiskAssessmentRequest 
                { 
                    MemberId = memberId,
                    RequestedAmount = 10000,
                    DurationInWeeks = 50
                });
                assessments.Add(assessment);
            }
            catch
            {
                continue;
            }
        }
        return new BatchRiskAssessmentResponse
        {
            Assessments = assessments,
            TotalProcessed = assessments.Count,
            LowRiskCount = assessments.Count(a => a.RiskScore < 30),
            MediumRiskCount = assessments.Count(a => a.RiskScore >= 30 && a.RiskScore < 60),
            HighRiskCount = assessments.Count(a => a.RiskScore >= 60)
        };
    }
}
