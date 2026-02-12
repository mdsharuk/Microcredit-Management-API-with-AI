using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Application.Services;
public class AIChatbotService : IAIChatbotService
{
    private readonly MicrocreditDbContext _context;
    public AIChatbotService(MicrocreditDbContext context)
    {
        _context = context;
    }
    public async Task<ChatbotResponse> ProcessMessageAsync(ChatbotRequest request)
    {
        var message = request.Message.ToLower().Trim();
        if (message.Contains("balance") || message.Contains("বাকি") || message.Contains("due"))
        {
            return await GetBalanceInquiryAsync(request.MemberId);
        }
        else if (message.Contains("payment") || message.Contains("next") || message.Contains("পরবর্তী"))
        {
            return await GetNextPaymentAsync(request.MemberId);
        }
        else if (message.Contains("status") || message.Contains("loan") || message.Contains("ঋণ"))
        {
            return await GetLoanStatusAsync(request.MemberId);
        }
        else
        {
            return new ChatbotResponse
            {
                Reply = request.Language == "bn"
                    ? "আমি আপনাকে সাহায্য করতে পারি। অনুগ্রহ করে বলুন:\n- ব্যালেন্স\n- পরবর্তী পেমেন্ট\n- ঋণের স্ট্যাটাস"
                    : "I can help you with:\n- Balance inquiry\n- Next payment details\n- Loan status",
                Intent = "help_menu",
                SuggestedQuestions = new List<string>
                {
                    "What's my balance?",
                    "When is my next payment?",
                    "Show my loan status"
                }
            };
        }
    }
    public async Task<ChatbotResponse> GetBalanceInquiryAsync(int memberId)
    {
        var activeLoan = await _context.Loans
            .Where(l => l.MemberId == memberId && 
                       (l.Status == Domain.Enums.LoanStatus.Active || 
                        l.Status == Domain.Enums.LoanStatus.Disbursed))
            .FirstOrDefaultAsync();
        if (activeLoan == null)
        {
            return new ChatbotResponse
            {
                Reply = "You don't have any active loan at the moment.",
                Intent = "balance_inquiry",
                Data = new Dictionary<string, object>
                {
                    { "hasActiveLoan", false }
                }
            };
        }
        var savingsAccount = await _context.SavingsAccounts
            .FirstOrDefaultAsync(s => s.MemberId == memberId);
        var reply = $@"💰 Your Account Summary:
Loan Balance: {activeLoan.RemainingBalance:N2} Taka
Total Paid: {activeLoan.PaidAmount:N2} Taka
Savings Balance: {savingsAccount?.Balance ?? 0:N2} Taka
Installments Paid: {activeLoan.PaidInstallments} of {activeLoan.DurationInWeeks}";
        return new ChatbotResponse
        {
            Reply = reply,
            Intent = "balance_inquiry",
            Data = new Dictionary<string, object>
            {
                { "loanBalance", activeLoan.RemainingBalance },
                { "paidAmount", activeLoan.PaidAmount },
                { "savingsBalance", savingsAccount?.Balance ?? 0 },
                { "paidInstallments", activeLoan.PaidInstallments },
                { "totalInstallments", activeLoan.DurationInWeeks }
            },
            SuggestedQuestions = new List<string>
            {
                "When is my next payment?",
                "Show payment history"
            }
        };
    }
    public async Task<ChatbotResponse> GetNextPaymentAsync(int memberId)
    {
        var activeLoan = await _context.Loans
            .Where(l => l.MemberId == memberId && 
                       (l.Status == Domain.Enums.LoanStatus.Active || 
                        l.Status == Domain.Enums.LoanStatus.Disbursed))
            .FirstOrDefaultAsync();
        if (activeLoan == null)
        {
            return new ChatbotResponse
            {
                Reply = "You don't have any active loan.",
                Intent = "next_payment"
            };
        }
        var nextInstallment = await _context.Installments
            .Where(i => i.LoanId == activeLoan.Id && 
                       i.Status == Domain.Enums.InstallmentStatus.Pending)
            .OrderBy(i => i.DueDate)
            .FirstOrDefaultAsync();
        if (nextInstallment == null)
        {
            return new ChatbotResponse
            {
                Reply = "🎉 Congratulations! All installments are paid.",
                Intent = "next_payment"
            };
        }
        var daysUntilDue = (nextInstallment.DueDate - DateTime.UtcNow).Days;
        var urgency = daysUntilDue <= 3 ? "⚠️ DUE SOON" : daysUntilDue < 0 ? "❌ OVERDUE" : "📅";
        var reply = $@"{urgency} Next Payment Details:
Amount: {nextInstallment.TotalAmount:N2} Taka
Due Date: {nextInstallment.DueDate:dd MMM yyyy}
Days Until Due: {daysUntilDue} days
Installment #: {nextInstallment.InstallmentNumber} of {activeLoan.DurationInWeeks}
Breakdown:
• Principal: {nextInstallment.PrincipalAmount:N2} Taka
• Interest: {nextInstallment.InterestAmount:N2} Taka
{(nextInstallment.FineAmount > 0 ? $"• Fine: {nextInstallment.FineAmount:N2} Taka" : "")}";
        return new ChatbotResponse
        {
            Reply = reply,
            Intent = "next_payment",
            Data = new Dictionary<string, object>
            {
                { "amount", nextInstallment.TotalAmount },
                { "dueDate", nextInstallment.DueDate },
                { "daysUntilDue", daysUntilDue },
                { "installmentNumber", nextInstallment.InstallmentNumber },
                { "isOverdue", daysUntilDue < 0 }
            },
            SuggestedQuestions = new List<string>
            {
                "Check my balance",
                "Show payment history",
                "Where to pay?"
            }
        };
    }
    public async Task<ChatbotResponse> GetLoanStatusAsync(int memberId)
    {
        var loans = await _context.Loans
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.ApplicationDate)
            .ToListAsync();
        if (!loans.Any())
        {
            return new ChatbotResponse
            {
                Reply = "You haven't applied for any loans yet.",
                Intent = "loan_status"
            };
        }
        var activeLoan = loans.FirstOrDefault(l => 
            l.Status == Domain.Enums.LoanStatus.Active || 
            l.Status == Domain.Enums.LoanStatus.Disbursed);
        if (activeLoan != null)
        {
            var progress = (activeLoan.PaidAmount / activeLoan.TotalPayable) * 100;
            var reply = $@"📊 Current Loan Status:
Loan Code: {activeLoan.LoanCode}
Status: {activeLoan.Status}
Progress: {progress:F1}% Complete
Original Amount: {activeLoan.LoanAmount:N2} Taka
Total Payable: {activeLoan.TotalPayable:N2} Taka
Paid So Far: {activeLoan.PaidAmount:N2} Taka
Remaining: {activeLoan.RemainingBalance:N2} Taka
Last Payment: {(activeLoan.LastPaymentDate.HasValue ? activeLoan.LastPaymentDate.Value.ToString("dd MMM yyyy") : "No payment yet")}
You have completed {activeLoan.PaidInstallments} of {activeLoan.DurationInWeeks} installments.";
            return new ChatbotResponse
            {
                Reply = reply,
                Intent = "loan_status",
                Data = new Dictionary<string, object>
                {
                    { "loanCode", activeLoan.LoanCode },
                    { "status", activeLoan.Status.ToString() },
                    { "progress", progress },
                    { "totalPayable", activeLoan.TotalPayable },
                    { "remaining", activeLoan.RemainingBalance }
                },
                SuggestedQuestions = new List<string>
                {
                    "When is next payment?",
                    "Check balance"
                }
            };
        }
        else
        {
            var lastLoan = loans.First();
            return new ChatbotResponse
            {
                Reply = $@"Your last loan ({lastLoan.LoanCode}) is {lastLoan.Status}.
You are eligible to apply for a new loan.
Would you like to know about loan options?",
                Intent = "loan_status",
                SuggestedQuestions = new List<string>
                {
                    "Loan options",
                    "Apply for new loan"
                }
            };
        }
    }
}
