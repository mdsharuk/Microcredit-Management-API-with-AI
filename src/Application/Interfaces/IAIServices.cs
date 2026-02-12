using Application.DTOs;
namespace Application.Interfaces;
/// <summary>
/// AI-powered risk assessment service for loan applications
/// Predicts probability of default and recommends approval/rejection
/// </summary>
public interface IAIRiskAssessmentService
{
    /// <summary>
    /// Assess risk for a single loan application
    /// </summary>
    Task<RiskAssessmentResponse> AssessRiskAsync(RiskAssessmentRequest request);
    /// <summary>
    /// Batch risk assessment for multiple members
    /// </summary>
    Task<BatchRiskAssessmentResponse> BatchAssessRiskAsync(BatchRiskAssessmentRequest request);
}
/// <summary>
/// AI-powered late payment prediction service
/// Predicts which members are likely to delay payments
/// </summary>
public interface IAIPaymentPredictionService
{
    /// <summary>
    /// Predict if an installment payment will be late
    /// </summary>
    Task<LatePaymentPredictionResponse> PredictLatePaymentAsync(LatePaymentPredictionRequest request);
    /// <summary>
    /// Get list of all upcoming installments with high delay risk
    /// </summary>
    Task<List<LatePaymentPredictionResponse>> GetHighRiskUpcomingPaymentsAsync(DateTime startDate, DateTime endDate);
    /// <summary>
    /// Send automated reminders to high-risk members
    /// </summary>
    Task SendAutomatedRemindersAsync(DateTime dueDate);
}
/// <summary>
/// AI-powered fraud detection service
/// Detects duplicate NIDs, suspicious patterns, and fraudulent activities
/// </summary>
public interface IAIFraudDetectionService
{
    /// <summary>
    /// Check for fraudulent patterns in member registration
    /// </summary>
    Task<FraudDetectionResponse> DetectFraudAsync(FraudDetectionRequest request);
    /// <summary>
    /// Check for duplicate NID across database
    /// </summary>
    Task<bool> CheckDuplicateNIDAsync(string nid);
    /// <summary>
    /// Validate phone number patterns
    /// </summary>
    Task<bool> ValidatePhonePatternAsync(string phone);
    /// <summary>
    /// Detect suspicious group formation patterns
    /// </summary>
    Task<FraudDetectionResponse> DetectSuspiciousGroupAsync(int groupId);
}
/// <summary>
/// AI-powered chatbot service for member inquiries
/// Provides automated responses to common questions
/// </summary>
public interface IAIChatbotService
{
    /// <summary>
    /// Process member message and generate AI response
    /// </summary>
    Task<ChatbotResponse> ProcessMessageAsync(ChatbotRequest request);
    /// <summary>
    /// Get member's loan balance
    /// </summary>
    Task<ChatbotResponse> GetBalanceInquiryAsync(int memberId);
    /// <summary>
    /// Get next payment details
    /// </summary>
    Task<ChatbotResponse> GetNextPaymentAsync(int memberId);
    /// <summary>
    /// Get loan status information
    /// </summary>
    Task<ChatbotResponse> GetLoanStatusAsync(int memberId);
}
/// <summary>
/// AI-powered collection optimization service
/// Analyzes performance and recommends improvements
/// </summary>
public interface IAICollectionOptimizationService
{
    /// <summary>
    /// Get comprehensive collection insights
    /// </summary>
    Task<CollectionOptimizationResponse> GetCollectionInsightsAsync(CollectionOptimizationRequest request);
    /// <summary>
    /// Identify underperforming branches
    /// </summary>
    Task<List<BranchPerformanceInsight>> GetUnderperformingBranchesAsync();
    /// <summary>
    /// Identify underperforming field officers
    /// </summary>
    Task<List<OfficerPerformanceInsight>> GetUnderperformingOfficersAsync();
    /// <summary>
    /// Identify high-risk geographical areas
    /// </summary>
    Task<List<HighRiskArea>> GetHighRiskAreasAsync();
    /// <summary>
    /// Predict next month's collection performance
    /// </summary>
    Task<decimal> PredictNextMonthCollectionAsync(int? branchId = null);
}
/// <summary>
/// Service to export training data for AI model development
/// </summary>
public interface IAIDataExportService
{
    /// <summary>
    /// Export historical data for AI model training
    /// </summary>
    Task<AITrainingDataResponse> ExportTrainingDataAsync(AITrainingDataRequest request);
    /// <summary>
    /// Export data in CSV format
    /// </summary>
    Task<byte[]> ExportAsCSVAsync(AITrainingDataRequest request);
    /// <summary>
    /// Export data in JSON format
    /// </summary>
    Task<string> ExportAsJSONAsync(AITrainingDataRequest request);
}
