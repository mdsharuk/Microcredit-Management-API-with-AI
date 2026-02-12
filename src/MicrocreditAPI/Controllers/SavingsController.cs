using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
using System.Security.Claims;
namespace MicrocreditAPI.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SavingsController : ControllerBase
{
    private readonly ISavingsService _savingsService;
    public SavingsController(ISavingsService savingsService)
    {
        _savingsService = savingsService;
    }
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] SavingsDepositDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var transaction = await _savingsService.DepositAsync(dto.AccountId, dto.Amount, userId, dto.Remarks);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] SavingsWithdrawalDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var transaction = await _savingsService.WithdrawAsync(dto.AccountId, dto.Amount, userId, dto.Remarks);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetAccountByMember(int memberId)
    {
        var account = await _savingsService.GetAccountByMemberIdAsync(memberId);
        if (account == null)
            return NotFound();
        return Ok(account);
    }
    [HttpGet("balance/{accountId}")]
    public async Task<IActionResult> GetBalance(int accountId)
    {
        var balance = await _savingsService.GetBalanceAsync(accountId);
        return Ok(new { balance });
    }
}
