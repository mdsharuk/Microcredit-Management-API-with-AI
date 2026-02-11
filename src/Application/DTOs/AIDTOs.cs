namespace Application.DTOs;
// AI Risk Assessment DTOs
public class RiskAssessmentRequest
{
    public Guid MemberId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int DurationInWeeks { get; set; }
}
public class RiskAssessmentResponse
{
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal RiskScore { get; set; } // 0-100 (0=safest, 100=riskiest)
    public string RiskCategory { get; set; } = string.Empty; // Low, Medium, High
    public bool IsApprovalRecommended { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> PositiveFactors { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}
// Late Payment Prediction DTOs
public class LatePaymentPredictionRequest
{
    public Guid MemberId { get; set; }
    public Guid LoanId { get; set; }
    public Guid InstallmentId { get; set; }
}
public class LatePaymentPredictionResponse
{
    public Guid InstallmentId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal ProbabilityOfDelay { get; set; } // 0-100%
    public string PredictionCategory { get; set; } = string.Empty; // OnTime, MayDelay, HighRisk
    public bool ShouldSendReminder { get; set; }
    public int RecommendedReminderDaysBefore { get; set; }
    public List<string> DelayRiskFactors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
// Fraud Detection DTOs
public class FraudDetectionRequest
{
    public Guid? MemberId { get; set; }
    public string? NID { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid? GroupId { get; set; }
}
public class FraudDetectionResponse
{
    public bool IsSuspicious { get; set; }
    public decimal FraudScore { get; set; } // 0-100 (0=safe, 100=fraud)
    public List<FraudAlert> Alerts { get; set; } = new();
    public string RecommendedAction { get; set; } = string.Empty;
}
public class FraudAlert
{
    public string Type { get; set; } = string.Empty; // DuplicateNID, SuspiciousPattern, etc.
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
}
// AI Chatbot DTOs
public class ChatbotRequest
{
    public Guid MemberId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Language { get; set; } = "en"; // en, bn
}
public class ChatbotResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty; // balance_inquiry, next_payment, loan_status
    public Dictionary<string, object>? Data { get; set; }
    public List<string> SuggestedQuestions { get; set; } = new();
}
// Collection Optimization DTOs
public class CollectionOptimizationRequest
{
    public Guid? BranchId { get; set; }
    public Guid? OfficerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
public class CollectionOptimizationResponse
{
    public List<BranchPerformanceInsight> BranchInsights { get; set; } = new();
    public List<OfficerPerformanceInsight> OfficerInsights { get; set; } = new();
    public List<HighRiskArea> HighRiskAreas { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public decimal OverallRecoveryRate { get; set; }
    public decimal PredictedNextMonthRecovery { get; set; }
}
public class BranchPerformanceInsight
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal RecoveryRate { get; set; }
    public decimal PortfolioAtRisk { get; set; } // PAR
    public string PerformanceCategory { get; set; } = string.Empty; // Excellent, Good, NeedsImprovement, Critical
    public List<string> Issues { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
}
public class OfficerPerformanceInsight
{
    public Guid OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public decimal CollectionEfficiency { get; set; }
    public int AssignedMembers { get; set; }
    public int OverdueAccounts { get; set; }
    public string PerformanceRating { get; set; } = string.Empty; // Excellent, Good, Average, Poor
    public List<string> Recommendations { get; set; } = new();
}
public class HighRiskArea
{
    public string AreaName { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public int DefaultedMembers { get; set; }
    public decimal DefaultRate { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Reason { get; set; } = string.Empty;
}
// Batch Prediction DTOs
public class BatchRiskAssessmentRequest
{
    public List<Guid> MemberIds { get; set; } = new();
}
public class BatchRiskAssessmentResponse
{
    public List<RiskAssessmentResponse> Assessments { get; set; } = new();
    public int TotalProcessed { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
}
// AI Training Data Export DTOs
public class AITrainingDataRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Features { get; set; } = new(); // member_info, loan_history, payment_behavior
}
public class AITrainingDataResponse
{
    public List<TrainingDataRecord> Records { get; set; } = new();
    public int TotalRecords { get; set; }
    public Dictionary<string, int> ClassDistribution { get; set; } = new();
}
public class TrainingDataRecord
{
    public Guid MemberId { get; set; }
    public decimal LoanAmount { get; set; }
    public int DurationInWeeks { get; set; }
    public int Age { get; set; }
    public decimal SavingsBalance { get; set; }
    public int PreviousLoansCount { get; set; }
    public int PreviousLatePaymentsCount { get; set; }
    public decimal AvgMonthlyIncome { get; set; }
    public int GroupPerformanceScore { get; set; }
    public bool HasDefaulted { get; set; } // Target variable
    public string LoanStatus { get; set; } = string.Empty;
}
