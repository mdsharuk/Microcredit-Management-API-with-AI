using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs;
namespace MicrocreditAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly MicrocreditDbContext _context;
    private readonly Application.Interfaces.ISavingsService _savingsService;
    public MembersController(MicrocreditDbContext context, Application.Interfaces.ISavingsService savingsService)
    {
        _context = context;
        _savingsService = savingsService;
    }
    [HttpGet]
    public async Task<IActionResult> GetMembers([FromQuery] int? branchId = null, [FromQuery] int? groupId = null)
    {
        var query = _context.Members
            .Include(m => m.Branch)
            .Include(m => m.Group)
            .Include(m => m.SavingsAccount)
            .AsQueryable();
        if (branchId.HasValue)
            query = query.Where(m => m.BranchId == branchId.Value);
        if (groupId.HasValue)
            query = query.Where(m => m.GroupId == groupId.Value);
        var members = await query.ToListAsync();
        return Ok(members);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _context.Members
            .Include(m => m.Branch)
            .Include(m => m.Group)
            .Include(m => m.SavingsAccount)
            .Include(m => m.Loans)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (member == null)
            return NotFound();
        return Ok(member);
    }
    [HttpPost]
    public async Task<IActionResult> CreateMember([FromBody] MemberRegistrationDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (await _context.Members.AnyAsync(m => m.NID == dto.NID))
            {
                return BadRequest(new { message = "NID already exists" });
            }
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == dto.GroupId);
            if (group == null)
                return BadRequest(new { message = "Group not found" });
            var memberCode = await GenerateMemberCodeAsync(dto.BranchId);
            var member = new Member
            {
                MemberCode = memberCode,
                FullName = dto.FullName,
                FatherName = dto.FatherName,
                MotherName = dto.MotherName,
                SpouseName = dto.SpouseName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                NID = dto.NID,
                Phone = dto.Phone,
                Address = dto.Address,
                Village = dto.Village,
                NomineeName = dto.NomineeName,
                NomineeRelation = dto.NomineeRelation,
                NomineePhone = dto.NomineePhone,
                NomineeNID = dto.NomineeNID,
                BranchId = dto.BranchId,
                GroupId = dto.GroupId,
                Status = MemberStatus.Active,
                JoinDate = DateTime.UtcNow,
                LoanCycle = 0,
                CreatedAt = DateTime.UtcNow
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            var savingsAccount = new SavingsAccount
            {
                MemberId = member.Id,
                CompulsoryWeeklySavings = 20
            };
            await _savingsService.CreateSavingsAccountAsync(savingsAccount);
            await transaction.CommitAsync();
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] Member member)
    {
        if (id != member.Id)
            return BadRequest();
        var existing = await _context.Members.FindAsync(id);
        if (existing == null)
            return NotFound();
        existing.FullName = member.FullName;
        existing.Phone = member.Phone;
        existing.Address = member.Address;
        existing.Status = member.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(existing);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
            return NotFound();
        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
    private async Task<string> GenerateMemberCodeAsync(int branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        var branchCode = branch?.BranchCode ?? "BR";
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.Members.CountAsync(m => m.BranchId == branchId) + 1;
        return $"MEM-{branchCode}-{year}-{count:D5}";
    }
}
