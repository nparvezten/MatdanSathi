using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatdanSathi.API.Controllers.v1;
using MatdanSathi.API.Domain.Entities;
using MatdanSathi.API.Infrastructure.Persistence;
using MatdanSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdanSathi.API.Tests;

public class WatchdogTests
{
    private readonly CryptographyService _cryptographyService;

    public WatchdogTests()
    {
        var settings = new CryptographySettings
        {
            EncryptionKey = "8sD3K5wGq9L2zR7vP1xB4cM6tJ0eY8uI",
            BlindIndexSalt = "mY-s3cr3t-s4lt-f0r-bl1nd-1ndex-HMAC-256"
        };
        var options = Options.Create(settings);
        _cryptographyService = new CryptographyService(options);
    }

    [Fact]
    public async Task Compare_Should_Correctly_Identify_Deletions_Transfers_And_AddressChanges()
    {
        // 1. Arrange: In-memory DB setup
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(dbOptions, _cryptographyService);

        // Seed 3 baseline voters:
        // Voter 1: Khan Saidnabi (Grandmother) - will be omitted (DELETED)
        // Voter 2: Ramesh Sawant - section will change from Section-1 to Section-2 (TRANSFERRED)
        // Voter 3: Deepa Joshi - house number will change from 43 to 43-B (ADDRESS CHANGED)
        var v1 = new VoterProfile
        {
            EpicNumber = "SLD1234567",
            FullName = "Khan Saidnabi",
            Age = 78,
            Gender = "Female",
            DateOfBirth = "05/06/1948",
            AssemblyConstituency = "Constituency-1",
            PartNumber = "Part-1",
            SectionNumber = "Section-1",
            SerialNumber = 12,
            PollingStationName = "Primary School East",
            PollingStationLocation = "Room 2",
            HouseNo = "42-A/1",
            BloName = "Prashant Patil",
            BloContact = "9876543210"
        };
        var v2 = new VoterProfile
        {
            EpicNumber = "SLD9876543",
            FullName = "Ramesh Sawant",
            Age = 45,
            Gender = "Male",
            DateOfBirth = "12/12/1980",
            AssemblyConstituency = "Constituency-1",
            PartNumber = "Part-1",
            SectionNumber = "Section-1",
            SerialNumber = 13,
            PollingStationName = "Primary School East",
            PollingStationLocation = "Room 2",
            HouseNo = "42-A/2",
            BloName = "Prashant Patil",
            BloContact = "9876543210"
        };
        var v3 = new VoterProfile
        {
            EpicNumber = "SLD2345678",
            FullName = "Deepa Joshi",
            Age = 29,
            Gender = "Female",
            DateOfBirth = "10/10/1996",
            AssemblyConstituency = "Constituency-1",
            PartNumber = "Part-1",
            SectionNumber = "Section-1",
            SerialNumber = 14,
            PollingStationName = "Primary School East",
            PollingStationLocation = "Room 2",
            HouseNo = "43",
            BloName = "Prashant Patil",
            BloContact = "9876543210"
        };

        context.VoterProfiles.Add(v1);
        context.VoterProfiles.Add(v2);
        context.VoterProfiles.Add(v3);
        await context.SaveChangesAsync();

        var controller = new WatchdogController(context, _cryptographyService);

        // Simulated parsed voters list from new roll:
        // - Saraswati Khan (Grandmother) is missing.
        // - Ramesh Sawant has moved to Section-2.
        // - Deepa Joshi house number changed to 43-B.
        var parsedVoters = new List<ParsedVoterDto>
        {
            new("SLD9876543", "Ramesh Sawant", 45, "Male", "Section-2", "42-A/2"),
            new("SLD2345678", "Deepa Joshi", 29, "Female", "Section-1", "43-B")
        };

        var request = new CompareElectoralRollsRequest("Constituency-1", "Part-1", parsedVoters);

        // 2. Act
        var result = await controller.CompareElectoralRolls(request);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var report = Assert.IsType<WatchdogReportDto>(okResult.Value);

        // Assert Grandmother (Khan Saidnabi) is flagged as deleted
        Assert.Single(report.Deletions);
        Assert.Equal("SLD1234567", report.Deletions[0].EpicNumber);
        Assert.Equal("Khan Saidnabi", report.Deletions[0].FullName);

        // Assert Ramesh Sawant is flagged as transferred
        Assert.Single(report.Transfers);
        Assert.Equal("SLD9876543", report.Transfers[0].EpicNumber);
        Assert.Equal("Section-1", report.Transfers[0].OldSectionNumber);
        Assert.Equal("Section-2", report.Transfers[0].NewSectionNumber);

        // Assert Deepa Joshi is flagged as address changed
        Assert.Single(report.AddressChanges);
        Assert.Equal("SLD2345678", report.AddressChanges[0].EpicNumber);
        Assert.Equal("43", report.AddressChanges[0].OldHouseNo);
        Assert.Equal("43-B", report.AddressChanges[0].NewHouseNo);
    }
}
