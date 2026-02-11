using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AIPaymentPredictionService : IAIPaymentPredictionService
{
    private readonly MicrocreditDbContext _context;
    public AIPaymentPredictionService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task<LatePaymentPredictionResponse> PredictLatePaymentAsync(LatePaymentPredictionRequest request)
    {
        var installment = await _context.Installments
            .Include(i => i.Loan)
                .ThenInclude(l => l.Member)
            .FirstOrDefaultAsync(i => i.Id == request.InstallmentId);
        if (installment == null)
            throw new Exception("Installment not found");
        var member = installment.Loan.Member;
        var delayRiskFactors = new List<string>();
        var delayProbability = 20m;
        var previousLatePayments = await _context.Installments
            .Where(i => i.Loan.MemberId == member.Id && 
                       i.LateDays > 0 && 
                       i.Status == Domain.Enums.InstallmentStatus.Paid)
            .CountAsync();
        if (previousLatePayments > 0)
        {
            var latePaymentRate = (decimal)previousLatePayments / 
                await _context.Installments.CountAsync(i => i.Loan.MemberId == member.Id);
            delayProbability += latePaymentRate * 40;
            delayRiskFactors.Add($"Previous late payment rate: {latePaymentRate:P0}");
        }
        var dueMonth = installment.DueDate.Month;
        if (dueMonth == 12 || dueMonth == 1 || dueMonth == 4)
        {
            delayProbability += 15;
            delayRiskFactors.Add("Due date in high-spending season");
        }
        if (installment.InstallmentNumber > 40)
        {
            delayProbability += 10;
            delayRiskFactors.Add("Late stage of loan repayment");
        }
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == member.GroupId);
        if (group != null && group.PerformanceRating < 0.6m)
        {
            delayProbability += 15;
            delayRiskFactors.Add("Group has below-average performance");
        }
        var recentPayments = await _context.Payments
            .Where(p => p.MemberId == member.Id)
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .ToListAsync();
        var recentLateCount = recentPayments.Count(p => 
            _context.Installments.Any(i => i.Id == p.InstallmentId && i.LateDays > 0));
        if (recentLateCount >= 3)
        {
            delayProbability += 20;
            delayRiskFactors.Add($"Recent trend: {recentLateCount} of last 5 payments were late");
        }
        delayProbability = Math.Min(100, delayProbability);
        string category;
        bool shouldRemind;
        int reminderDays;
        if (delayProbability < 30)
        {
            category = "OnTime";
            shouldRemind = false;
            reminderDays = 1;
        }
        else if (delayProbability < 60)
        {
            category = "MayDelay";
            shouldRemind = true;
            reminderDays = 3;
        }
        else
        {
            category = "HighRisk";
            shouldRemind = true;
            reminderDays = 5;
        }
        var message = category switch
        {
            "OnTime" => "Member has strong payment track record. Standard reminder is sufficient.",
            "MayDelay" => "Member shows some delay risk factors. Send early reminder.",
            "HighRisk" => "High probability of delay. Send multiple reminders and consider field visit.",
            _ => ""
        };
        return new LatePaymentPredictionResponse
        {
            InstallmentId = installment.Id,
            DueDate = installment.DueDate,
            Amount = installment.TotalAmount,
            ProbabilityOfDelay = Math.Round(delayProbability, 2),
            PredictionCategory = category,
            ShouldSendReminder = shouldRemind,
            RecommendedReminderDaysBefore = reminderDays,
            DelayRiskFactors = delayRiskFactors,
            Message = message
        };
    }
    public async Task<List<LatePaymentPredictionResponse>> GetHighRiskUpcomingPaymentsAsync(
        DateTime startDate, DateTime endDate)
    {
        var upcomingInstallments = await _context.Installments
            .Include(i => i.Loan)
                .ThenInclude(l => l.Member)
            .Where(i => i.DueDate >= startDate && 
                       i.DueDate <= endDate && 
                       i.Status == Domain.Enums.InstallmentStatus.Pending)
            .ToListAsync();
        var predictions = new List<LatePaymentPredictionResponse>();
        foreach (var installment in upcomingInstallments)
        {
            try
            {
                var prediction = await PredictLatePaymentAsync(new LatePaymentPredictionRequest
                {
                    MemberId = installment.Loan.MemberId,
                    LoanId = installment.LoanId,
                    InstallmentId = installment.Id
                });
                if (prediction.ProbabilityOfDelay >= 50)
                {
                    predictions.Add(prediction);
                }
            }
            catch
            {
                continue;
            }
        }
        return predictions.OrderByDescending(p => p.ProbabilityOfDelay).ToList();
    }
    public async Task SendAutomatedRemindersAsync(DateTime dueDate)
    {
        var predictions = await GetHighRiskUpcomingPaymentsAsync(
            dueDate.AddDays(-7), 
            dueDate);
        foreach (var prediction in predictions)
        {
            if (prediction.ShouldSendReminder)
            {
                Console.WriteLine($"Reminder sent for installment {prediction.InstallmentId}");
            }
        }
        await Task.CompletedTask;
    }
}
