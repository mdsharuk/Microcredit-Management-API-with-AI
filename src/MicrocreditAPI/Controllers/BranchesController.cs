using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using Domain.Enums;
namespace MicrocreditAPI.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly MicrocreditDbContext _context;
    public BranchesController(MicrocreditDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetBranches()
    {
        var branches = await _context.Branches
            .Include(b => b.Manager)
            .ToListAsync();
        return Ok(branches);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBranch(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.Manager)
            .Include(b => b.Staff)
            .Include(b => b.Groups)
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null)
            return NotFound();
        return Ok(branch);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBranch([FromBody] Branch branch)
    {
        branch.CreatedAt = DateTime.UtcNow;
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, branch);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBranch(int id, [FromBody] Branch branch)
    {
        if (id != branch.Id)
            return BadRequest();
        var existing = await _context.Branches.FindAsync(id);
        if (existing == null)
            return NotFound();
        existing.BranchName = branch.BranchName;
        existing.Address = branch.Address;
        existing.Phone = branch.Phone;
        existing.Email = branch.Email;
        existing.IsActive = branch.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
