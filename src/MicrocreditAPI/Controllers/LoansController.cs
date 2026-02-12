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
                BranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0")
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
    public async Task<IActionResult> GetLoan(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);
        if (loan == null)
            return NotFound();
        return Ok(loan);
    }
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetMemberLoans(int memberId)
    {
        var loans = await _loanService.GetLoansByMemberIdAsync(memberId);
        return Ok(loans);
    }
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> GetPendingLoans([FromQuery] int? branchId = null)
    {
        var loans = await _loanService.GetPendingLoansAsync(branchId);
        return Ok(loans);
    }
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<IActionResult> ApproveLoan(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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
    public async Task<IActionResult> DisburseLoan(int id)
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
    public async Task<IActionResult> GetInstallmentSchedule(int id)
    {
        var schedule = await _loanService.GetInstallmentScheduleAsync(id);
        return Ok(schedule);
    }
}
