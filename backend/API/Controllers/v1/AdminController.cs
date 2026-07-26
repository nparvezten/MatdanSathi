using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;

    public AdminController(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("volunteers")]
    public async Task<IActionResult> GetVolunteers()
    {
        var volunteers = await _dbContext.UserVerifiers
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.AssemblyConstituency,
                u.Role,
                u.Status,
                u.CreatedAt,
                u.ApprovedAt
            })
            .ToListAsync();

        return Ok(volunteers);
    }

    [HttpPost("approve-volunteer")]
    public async Task<IActionResult> ApproveVolunteer([FromBody] UserActionRequest request)
    {
        var user = await _dbContext.UserVerifiers.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new { Message = "Volunteer record not found." });
        }

        user.Status = "Approved";
        user.ApprovedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = $"Volunteer {user.FullName} ({user.Email}) has been successfully approved!", Status = "Approved" });
    }

    [HttpPost("reject-volunteer")]
    public async Task<IActionResult> RejectVolunteer([FromBody] UserActionRequest request)
    {
        var user = await _dbContext.UserVerifiers.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new { Message = "Volunteer record not found." });
        }

        user.Status = "Rejected";
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = $"Volunteer application for {user.FullName} was rejected.", Status = "Rejected" });
    }
}

public record UserActionRequest(int UserId);
