using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;
using Application.DTOs;
using Domain.Entities;
using System.Security.Claims;
namespace MicrocreditAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }
    [HttpPost]
    public async Task<IActionResult> CreateLoanApplication([FromBody] LoanApplicationDto dto)
    {
        try
        {
            var loan = new Loan
            {
                MemberId = dto.MemberId,
                LoanType = dto.LoanType,
                LoanAmount = dto.LoanAmount,
                InterestRate = dto.InterestRate,
                InterestType = dto.InterestType,
                DurationInWeeks = dto.DurationInWeeks,
                Purpose = dto.Purpose,
                BranchId = Guid.Parse(User.FindFirst("BranchId")?.Value ?? Guid.Empty.ToString())
            };
            var createdLoan = await _loanService.CreateLoanApplicationAsync(loan);
            return CreatedAtAction(nameof(GetLoan), new { id = createdLoan.Id }, createdLoan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLoan(Guid id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);
        if (loan == null)
            return NotFound();
        return Ok(loan);
    }
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetMemberLoans(Guid memberId)
    {
        var loans = await _loanService.GetLoansByMemberIdAsync(memberId);
        return Ok(loans);
    }
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> GetPendingLoans([FromQuery] Guid? branchId = null)
    {
        var loans = await _loanService.GetPendingLoansAsync(branchId);
        return Ok(loans);
    }
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> ApproveLoan(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var loan = await _loanService.ApproveLoanAsync(id, userId);
            return Ok(loan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPut("{id}/disburse")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> DisburseLoan(Guid id)
    {
        try
        {
            var loan = await _loanService.DisburseLoanAsync(id);
            return Ok(loan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetInstallmentSchedule(Guid id)
    {
        var schedule = await _loanService.GetInstallmentScheduleAsync(id);
        return Ok(schedule);
    }
}
