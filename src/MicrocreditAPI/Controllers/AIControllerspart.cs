using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
namespace MicrocreditAPI.Controllers;
[Authorize]
[ApiController]
[Route("api/ai/[controller]")]
[Produces("application/json")]
public class RiskAssessmentController : ControllerBase
{
    private readonly IAIRiskAssessmentService _riskService;
    public RiskAssessmentController(IAIRiskAssessmentService riskService)
    {
        _riskService = riskService;
    }
    [HttpPost("assess")]
    [ProducesResponseType(typeof(RiskAssessmentResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssessRisk([FromBody] RiskAssessmentRequest request)
    {
        try
        {
            var result = await _riskService.AssessRiskAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost("batch-assess")]
    [Authorize(Roles = "Admin,BranchManager")]
    [ProducesResponseType(typeof(BatchRiskAssessmentResponse), 200)]
    public async Task<IActionResult> BatchAssess([FromBody] BatchRiskAssessmentRequest request)
    {
        var result = await _riskService.BatchAssessRiskAsync(request);
        return Ok(result);
    }
}
[Authorize]
[ApiController]
[Route("api/ai/[controller]")]
public class PaymentPredictionController : ControllerBase
{
    private readonly IAIPaymentPredictionService _predictionService;
    public PaymentPredictionController(IAIPaymentPredictionService predictionService)
    {
        _predictionService = predictionService;
    }
    [HttpPost("predict")]
    [ProducesResponseType(typeof(LatePaymentPredictionResponse), 200)]
    public async Task<IActionResult> PredictLatePayment([FromBody] LatePaymentPredictionRequest request)
    {
        try
        {
            var result = await _predictionService.PredictLatePaymentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("high-risk")]
    [Authorize(Roles = "Admin,BranchManager,FieldOfficer")]
    [ProducesResponseType(typeof(List<LatePaymentPredictionResponse>), 200)]
    public async Task<IActionResult> GetHighRiskPayments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow;
        var end = endDate ?? DateTime.UtcNow.AddDays(30);
        var result = await _predictionService.GetHighRiskUpcomingPaymentsAsync(start, end);
        return Ok(result);
    }
    [HttpPost("send-reminders")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> SendReminders([FromQuery] DateTime dueDate)
    {
        await _predictionService.SendAutomatedRemindersAsync(dueDate);
        return Ok(new { message = "Reminders sent successfully" });
    }
}
[Authorize]
[ApiController]
[Route("api/ai/[controller]")]
public class FraudDetectionController : ControllerBase
{
    private readonly IAIFraudDetectionService _fraudService;
    public FraudDetectionController(IAIFraudDetectionService fraudService)
    {
        _fraudService = fraudService;
    }
    [HttpPost("detect")]
    [ProducesResponseType(typeof(FraudDetectionResponse), 200)]
    public async Task<IActionResult> DetectFraud([FromBody] FraudDetectionRequest request)
    {
        var result = await _fraudService.DetectFraudAsync(request);
        return Ok(result);
    }
    [HttpGet("check-nid/{nid}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> CheckDuplicateNID(string nid)
    {
        var exists = await _fraudService.CheckDuplicateNIDAsync(nid);
        return Ok(new { exists, nid });
    }
    [HttpGet("validate-phone/{phone}")]
    public async Task<IActionResult> ValidatePhone(string phone)
    {
        var isValid = await _fraudService.ValidatePhonePatternAsync(phone);
        return Ok(new { isValid, phone });
    }
    [HttpGet("group/{groupId}")]
    [Authorize(Roles = "Admin,BranchManager")]
    [ProducesResponseType(typeof(FraudDetectionResponse), 200)]
    public async Task<IActionResult> DetectSuspiciousGroup(Guid groupId)
    {
        try
        {
            var result = await _fraudService.DetectSuspiciousGroupAsync(groupId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
[Authorize]
[ApiController]
[Route("api/ai/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IAIChatbotService _chatbotService;
    public ChatbotController(IAIChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatbotResponse), 200)]
    public async Task<IActionResult> Chat([FromBody] ChatbotRequest request)
    {
        var result = await _chatbotService.ProcessMessageAsync(request);
        return Ok(result);
    }
    [HttpGet("balance/{memberId}")]
    [ProducesResponseType(typeof(ChatbotResponse), 200)]
    public async Task<IActionResult> GetBalance(Guid memberId)
    {
        var result = await _chatbotService.GetBalanceInquiryAsync(memberId);
        return Ok(result);
    }
    [HttpGet("next-payment/{memberId}")]
    [ProducesResponseType(typeof(ChatbotResponse), 200)]
    public async Task<IActionResult> GetNextPayment(Guid memberId)
    {
        var result = await _chatbotService.GetNextPaymentAsync(memberId);
        return Ok(result);
    }
    [HttpGet("loan-status/{memberId}")]
    [ProducesResponseType(typeof(ChatbotResponse), 200)]
    public async Task<IActionResult> GetLoanStatus(Guid memberId)
    {
        var result = await _chatbotService.GetLoanStatusAsync(memberId);
        return Ok(result);
    }
}
