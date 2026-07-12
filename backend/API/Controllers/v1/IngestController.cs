using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Domain.Entities;

namespace MatdanSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class IngestController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly string _expectedApiKey;

    public IngestController(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        // Fetch internal token for service-to-service validation
        _expectedApiKey = configuration["CryptographySettings:BlindIndexSalt"]
                          ?? "matdansathi-secure-internal-token-2026";
    }

    [HttpPost("voters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IngestVoters(
        [FromBody] List<IngestVoterDto> voters,
        [FromHeader(Name = "X-API-KEY")] string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey != _expectedApiKey)
        {
            return Unauthorized("Invalid or missing X-API-KEY header.");
        }

        if (voters == null || voters.Count == 0)
        {
            return BadRequest("No voter records provided for ingestion.");
        }

        foreach (var dto in voters)
        {
            var voter = new VoterProfile
            {
                EpicNumber = dto.EpicNumber,
                FullName = dto.FullName,
                Age = dto.Age,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                AssemblyConstituency = dto.AssemblyConstituency,
                PartNumber = dto.PartNumber,
                SectionNumber = dto.SectionNumber,
                SerialNumber = dto.SerialNumber,
                PollingStationName = dto.PollingStationName,
                PollingStationLocation = dto.PollingStationLocation,
                HouseNo = dto.HouseNo,
                BloName = dto.BloName,
                BloContact = dto.BloContact
            };

            _context.VoterProfiles.Add(voter);
        }

        await _context.SaveChangesAsync(default);

        return Ok(new { Count = voters.Count, Message = "Voter profiles successfully ingested and secure-stored." });
    }
}

public record IngestVoterDto(
    string EpicNumber,
    string FullName,
    int Age,
    string DateOfBirth,
    string Gender,
    string AssemblyConstituency,
    string PartNumber,
    string SectionNumber,
    int SerialNumber,
    string PollingStationName,
    string PollingStationLocation,
    string HouseNo,
    string BloName,
    string BloContact
);
