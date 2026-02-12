using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using System.Security.Claims;
namespace MicrocreditAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] PaymentDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var payment = new Payment
            {
                LoanId = dto.LoanId,
                MemberId = dto.MemberId,
                InstallmentId = dto.InstallmentId,
                TotalAmount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                CollectedBy = dto.CollectedBy,
                Remarks = dto.Remarks
            };
            var recordedPayment = await _paymentService.RecordPaymentAsync(payment);
            return Ok(recordedPayment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("loan/{loanId}")]
    public async Task<IActionResult> GetLoanPayments(int loanId)
    {
        var payments = await _paymentService.GetPaymentsByLoanIdAsync(loanId);
        return Ok(payments);
    }
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetMemberPayments(int memberId)
    {
        var payments = await _paymentService.GetPaymentsByMemberIdAsync(memberId);
        return Ok(payments);
    }
    [HttpGet("installment/{installmentId}/fine")]
    public async Task<IActionResult> CalculateFine(int installmentId)
    {
        var fine = await _paymentService.CalculateFineAsync(installmentId);
        return Ok(new { installmentId, fine });
    }
}
