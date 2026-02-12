using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
namespace MicrocreditAPI.Controllers;
[Authorize(Roles = "Admin,BranchManager")]
[ApiController]
[Route("api/ai/[controller]")]
[Produces("application/json")]
public class CollectionOptimizationController : ControllerBase
{
    private readonly IAICollectionOptimizationService _optimizationService;
    public CollectionOptimizationController(IAICollectionOptimizationService optimizationService)
    {
        _optimizationService = optimizationService;
    }
    [HttpPost("insights")]
    [ProducesResponseType(typeof(CollectionOptimizationResponse), 200)]
    public async Task<IActionResult> GetInsights([FromBody] CollectionOptimizationRequest request)
    {
        var result = await _optimizationService.GetCollectionInsightsAsync(request);
        return Ok(result);
    }
    [HttpGet("underperforming-branches")]
    [ProducesResponseType(typeof(List<BranchPerformanceInsight>), 200)]
    public async Task<IActionResult> GetUnderperformingBranches()
    {
        var result = await _optimizationService.GetUnderperformingBranchesAsync();
        return Ok(result);
    }
    [HttpGet("underperforming-officers")]
    [ProducesResponseType(typeof(List<OfficerPerformanceInsight>), 200)]
    public async Task<IActionResult> GetUnderperformingOfficers()
    {
        var result = await _optimizationService.GetUnderperformingOfficersAsync();
        return Ok(result);
    }
    [HttpGet("high-risk-areas")]
    [ProducesResponseType(typeof(List<HighRiskArea>), 200)]
    public async Task<IActionResult> GetHighRiskAreas()
    {
        var result = await _optimizationService.GetHighRiskAreasAsync();
        return Ok(result);
    }
    [HttpGet("predict-collection")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<IActionResult> PredictNextMonthCollection([FromQuery] int? branchId = null)
    {
        var predicted = await _optimizationService.PredictNextMonthCollectionAsync(branchId);
        return Ok(new 
        { 
            predictedAmount = predicted,
            period = "next_30_days",
            branchId = branchId,
            currency = "BDT"
        });
    }
}
