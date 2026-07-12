using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using MatdanSathi.API.Application.Common.Interfaces;

namespace MatdanSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class WatchdogController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public WatchdogController(IApplicationDbContext context, ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    [HttpPost("compare")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WatchdogReportDto))]
    public async Task<IActionResult> CompareElectoralRolls([FromBody] CompareElectoralRollsRequest request)
    {
        if (string.IsNullOrEmpty(request.AssemblyConstituency) || string.IsNullOrEmpty(request.PartNumber))
        {
            return BadRequest("AssemblyConstituency and PartNumber are required.");
        }

        // 1. Fetch database baseline voters for this constituency and part
        var dbVoters = await _context.VoterProfiles
            .Where(v => v.AssemblyConstituency == request.AssemblyConstituency && v.PartNumber == request.PartNumber && !v.IsDeleted)
            .ToListAsync();

        // 2. Generate blind indexes for incoming parsed voters
        var incomingMap = request.ParsedVoters.ToDictionary(
            pv => _cryptographyService.GenerateBlindIndex(pv.EpicNumber.Trim().ToUpperInvariant()),
            pv => pv
        );

        var deletions = new List<DeletedVoterDto>();
        var transfers = new List<TransferredVoterDto>();
        var addressChanges = new List<AddressChangedVoterDto>();

        // 3. Compare baseline voters against incoming parsed roll
        foreach (var dbVoter in dbVoters)
        {
            if (!incomingMap.TryGetValue(dbVoter.EpicNumberBlindIndex, out var parsedVoter))
            {
                // Voter in database baseline is MISSING in new parsed roll -> DELETED
                deletions.Add(new DeletedVoterDto(
                    _cryptographyService.Decrypt(dbVoter.EpicNumberEncrypted),
                    _cryptographyService.Decrypt(dbVoter.FullNameEncrypted),
                    dbVoter.Age,
                    dbVoter.Gender,
                    dbVoter.SectionNumber,
                    dbVoter.HouseNo
                ));
            }
            else
            {
                bool isTransferred = dbVoter.SectionNumber != parsedVoter.SectionNumber;
                bool isAddressChanged = dbVoter.HouseNo != parsedVoter.HouseNo;

                if (isTransferred)
                {
                    transfers.Add(new TransferredVoterDto(
                        _cryptographyService.Decrypt(dbVoter.EpicNumberEncrypted),
                        _cryptographyService.Decrypt(dbVoter.FullNameEncrypted),
                        dbVoter.SectionNumber,
                        parsedVoter.SectionNumber
                    ));
                }

                if (isAddressChanged)
                {
                    addressChanges.Add(new AddressChangedVoterDto(
                        _cryptographyService.Decrypt(dbVoter.EpicNumberEncrypted),
                        _cryptographyService.Decrypt(dbVoter.FullNameEncrypted),
                        dbVoter.HouseNo,
                        parsedVoter.HouseNo
                    ));
                }
            }
        }

        var report = new WatchdogReportDto(
            request.AssemblyConstituency,
            request.PartNumber,
            deletions,
            transfers,
            addressChanges
        );

        return Ok(report);
    }
}

public record CompareElectoralRollsRequest(
    string AssemblyConstituency,
    string PartNumber,
    List<ParsedVoterDto> ParsedVoters
);

public record ParsedVoterDto(
    string EpicNumber,
    string FullName,
    int Age,
    string Gender,
    string SectionNumber,
    string HouseNo
);

public record WatchdogReportDto(
    string AssemblyConstituency,
    string PartNumber,
    List<DeletedVoterDto> Deletions,
    List<TransferredVoterDto> Transfers,
    List<AddressChangedVoterDto> AddressChanges
);

public record DeletedVoterDto(
    string EpicNumber,
    string FullName,
    int Age,
    string Gender,
    string SectionNumber,
    string HouseNo
);

public record TransferredVoterDto(
    string EpicNumber,
    string FullName,
    string OldSectionNumber,
    string NewSectionNumber
);

public record AddressChangedVoterDto(
    string EpicNumber,
    string FullName,
    string OldHouseNo,
    string NewHouseNo
);
