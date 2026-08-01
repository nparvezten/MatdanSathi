using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;

namespace MatdarSathi.API.Infrastructure.Services;

public class WatchdogComparisonService : IWatchdogComparisonService
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptoService;

    public WatchdogComparisonService(
        IApplicationDbContext context,
        ICryptographyService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<int> CompareAndIngestParsedRecordsAsync(
        string boothId,
        List<ParsedVoterRecord> records,
        CancellationToken cancellationToken = default)
    {
        if (records == null || records.Count == 0)
        {
            return 0;
        }

        int count = 0;
        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.EpicNumber))
            {
                continue;
            }

            var cleanEpic = record.EpicNumber.Trim().ToUpperInvariant();
            var blindIndex = _cryptoService.GenerateBlindIndex(cleanEpic);

            var existing = await _context.VoterProfiles
                .FirstOrDefaultAsync(v => v.EpicNumberBlindIndex == blindIndex && !v.IsDeleted, cancellationToken);

            int.TryParse(record.SerialNumber, out int parsedSerialNo);
            if (parsedSerialNo <= 0) parsedSerialNo = count + 1;

            if (existing == null)
            {
                var profile = new VoterProfile
                {
                    EpicNumber = cleanEpic,
                    FullName = record.FullName,
                    DateOfBirth = "1990-01-01",
                    BloContact = "1950",
                    BloName = "BLO Official",
                    Age = record.Age,
                    Gender = string.IsNullOrWhiteSpace(record.Gender) ? "M" : record.Gender,
                    AssemblyConstituency = record.AssemblyConstituency ?? "Assembly 182",
                    PartNumber = string.IsNullOrWhiteSpace(record.PartNumber) ? boothId : record.PartNumber,
                    SectionNumber = record.SectionNumber ?? "Section-1",
                    SerialNumber = parsedSerialNo,
                    PollingStationName = record.PollingStationName ?? "Booth Station",
                    PollingStationLocation = record.PollingStationLocation ?? "Local Booth",
                    HouseNo = record.HouseNo ?? "N/A"
                };

                _context.VoterProfiles.Add(profile);
                count++;
            }
            else
            {
                existing.AssemblyConstituency = record.AssemblyConstituency ?? existing.AssemblyConstituency;
                existing.PartNumber = string.IsNullOrWhiteSpace(record.PartNumber) ? boothId : record.PartNumber;
                count++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return count;
    }
}
