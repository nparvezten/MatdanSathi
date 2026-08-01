using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdarSathi.API.Application.Anomalies.Commands.SubmitLegacyAnomaly;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace MatdarSathi.API.Tests;

public class AnomalyTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var cryptoSettings = Options.Create(new CryptographySettings
        {
            EncryptionKey = "12345678901234567890123456789012",
            BlindIndexSalt = "test-salt-secret-key-123456"
        });

        var cryptoService = new CryptographyService(cryptoSettings);
        return new ApplicationDbContext(options, cryptoService);
    }

    [Fact]
    public async Task SubmitLegacyAnomalyCommand_ValidInput_EncryptsPIIAndSavesRecord()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new SubmitLegacyAnomalyCommandHandler(dbContext);

        var familyMembers = new List<FamilyMemberDto>
        {
            new FamilyMemberDto("Khan Saidnabi", "Head/Deceased", 78, "SLD1234567"),
            new FamilyMemberDto("Parvez Khan", "Grandson", 30, "SLD9876543")
        };

        var command = new SubmitLegacyAnomalyCommand(
            ReceiptNumber: "CERT-EXT-2026-9901",
            DeceasedName: "Khan Saidnabi",
            YearOfDeath: 1997,
            DeathCertRegNo: "MCGM-DEATH-1997-8812",
            ConstituencyName: "182 - Sion Koliwada",
            PartNumber: "Part 14",
            PageNumber: "Page 8",
            SerialRange: "Serial 102 - 108",
            FamilyMembers: familyMembers
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CERT-EXT-2026-9901", result.ReceiptNumber);
        Assert.Equal("LoggedSuccessfully", result.Status);

        var savedRecord = await dbContext.LegacyAnomalyRecords.FirstOrDefaultAsync(r => r.Id == result.Id);
        Assert.NotNull(savedRecord);
        Assert.NotNull(savedRecord.DeceasedNameBlindIndex);
        Assert.NotNull(savedRecord.DeceasedNameEncrypted);
        Assert.NotNull(savedRecord.DeathCertRegNoEncrypted);
        Assert.NotNull(savedRecord.FamilyBundleJsonEncrypted);
        Assert.NotEqual("Khan Saidnabi", savedRecord.DeceasedNameEncrypted); // PII is encrypted!
    }

    [Fact]
    public void SubmitLegacyAnomalyCommandValidator_EmptyReceipt_FailsValidation()
    {
        // Arrange
        var validator = new SubmitLegacyAnomalyCommandValidator();
        var command = new SubmitLegacyAnomalyCommand(
            ReceiptNumber: "",
            DeceasedName: "Khan Saidnabi",
            YearOfDeath: 1997,
            DeathCertRegNo: "MCGM-DEATH-1997-8812",
            ConstituencyName: "182 - Sion Koliwada",
            PartNumber: "Part 14",
            PageNumber: "Page 8",
            SerialRange: "Serial 102 - 108",
            FamilyMembers: new List<FamilyMemberDto>()
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ReceiptNumber");
    }
}
