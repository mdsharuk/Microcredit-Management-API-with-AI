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
public class GroupsController : ControllerBase
{
    private readonly MicrocreditDbContext _context;
    public GroupsController(MicrocreditDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetGroups([FromQuery] Guid? branchId = null)
    {
        var query = _context.Groups
            .Include(g => g.Branch)
            .Include(g => g.FieldOfficer)
            .Include(g => g.Leader)
            .Include(g => g.Members)
            .AsQueryable();
        if (branchId.HasValue)
            query = query.Where(g => g.BranchId == branchId.Value);
        var groups = await query.ToListAsync();
        return Ok(groups);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var group = await _context.Groups
            .Include(g => g.Branch)
            .Include(g => g.FieldOfficer)
            .Include(g => g.Leader)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null)
            return NotFound();
        return Ok(group);
    }
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] GroupCreationDto dto)
    {
        var groupCode = await GenerateGroupCodeAsync(dto.BranchId);
        var group = new Group
        {
            Id = Guid.NewGuid(),
            GroupCode = groupCode,
            GroupName = dto.GroupName,
            BranchId = dto.BranchId,
            FieldOfficerId = dto.FieldOfficerId,
            MeetingDay = dto.MeetingDay,
            MeetingTime = dto.MeetingTime,
            Status = GroupStatus.Forming,
            MinMembers = 5,
            PerformanceRating = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateGroup(Guid id)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null)
            return NotFound();
        if (group.Members.Count < group.MinMembers)
        {
            return BadRequest(new { message = $"Group must have at least {group.MinMembers} members to activate" });
        }
        if (!group.LeaderId.HasValue)
        {
            return BadRequest(new { message = "Group must have a leader to activate" });
        }
        group.Status = GroupStatus.Active;
        group.ActivationDate = DateTime.UtcNow;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(group);
    }
    [HttpPut("{id}/set-leader/{memberId}")]
    public async Task<IActionResult> SetGroupLeader(Guid id, Guid memberId)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound();
        var member = await _context.Members.FindAsync(memberId);
        if (member == null || member.GroupId != id)
        {
            return BadRequest(new { message = "Member not found or not in this group" });
        }
        group.LeaderId = memberId;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(group);
    }
    private async Task<string> GenerateGroupCodeAsync(Guid branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        var branchCode = branch?.BranchCode ?? "BR";
        var year = DateTime.UtcNow.Year.ToString().Substring(2);
        var count = await _context.Groups.CountAsync(g => g.BranchId == branchId) + 1;
        return $"GRP-{branchCode}-{year}-{count:D4}";
    }
}
