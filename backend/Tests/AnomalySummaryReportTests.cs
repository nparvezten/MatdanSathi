using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Reports.Queries.GetAnomalySummaryByBooth;
using MatdarSathi.API.Controllers.v1;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Infrastructure.Common;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdarSathi.API.Tests;

public class AnomalySummaryReportTests
{
    private (ApplicationDbContext dbContext, CryptographyService cryptoService) CreateTestContext()
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
        var dbContext = new ApplicationDbContext(options, cryptoService);
        return (dbContext, cryptoService);
    }

    [Fact]
    public async Task GetAnomalySummary_ReturnsAggregatedCounts_WithoutExposingPII()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();

        // Add legacy anomaly record with sensitive PII in encrypted fields
        dbContext.LegacyAnomalyRecords.Add(new LegacyAnomalyRecord
        {
            ReceiptNumber = "BOOTH-182",
            ConstituencyName = "182-Mumbai",
            PartNumber = "Part-1",
            PageNumber = "Page-2",
            SerialRange = "100-110",
            YearOfDeath = 2024,
            DeceasedNameEncrypted = cryptoService.Encrypt("Sensitive Citizen Name"),
            DeceasedNameBlindIndex = cryptoService.GenerateBlindIndex("Sensitive Citizen Name"),
            DeathCertRegNoEncrypted = cryptoService.Encrypt("REG-998877"),
            FamilyBundleJsonEncrypted = cryptoService.Encrypt("{\"voter\":\"Secret PII Data\"}")
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetAnomalySummaryByBoothQueryHandler(dbContext);
        var query = new GetAnomalySummaryByBoothQuery("Mumbai City");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mumbai City", result.District);
        Assert.True(result.OverallTotalAnomalies > 0);
        Assert.NotEmpty(result.BoothSummaries);
        Assert.Contains("DISCLAIMER", result.DisclaimerNotice);

        // DEFENSIVE PII SCAN ASSERTION:
        // Serialize the entire report object to JSON and verify zero sensitive PII substrings exist
        string jsonOutput = JsonSerializer.Serialize(result);

        Assert.DoesNotContain("Sensitive Citizen Name", jsonOutput);
        Assert.DoesNotContain("REG-998877", jsonOutput);
        Assert.DoesNotContain("Secret PII Data", jsonOutput);
        Assert.DoesNotContain("EpicNumber", jsonOutput);
        Assert.DoesNotContain("FullName", jsonOutput);
        Assert.DoesNotContain("DateOfBirth", jsonOutput);
        Assert.DoesNotContain("BloContact", jsonOutput);

        // Ensure property types are only string (district/booth ID/disclaimer) or int counts
        foreach (var booth in result.BoothSummaries)
        {
            var properties = booth.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var val = prop.GetValue(booth);
                Assert.True(prop.PropertyType == typeof(string) || prop.PropertyType == typeof(int),
                    $"Unexpected non-aggregate property type '{prop.PropertyType.Name}' on BoothAnomalySummaryDto");

                if (prop.PropertyType == typeof(string))
                {
                    string strVal = (string)val!;
                    Assert.True(strVal == booth.BoothId || strVal == booth.District,
                        $"String property '{prop.Name}' in BoothAnomalySummaryDto contains non-booth data: '{strVal}'");
                }
            }
        }
    }

    [Fact]
    public async Task ReportsController_GetAnomalySummary_Returns200OK()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        var mediator = new NativeMediator(new ReportTestServiceProvider(dbContext));
        var controller = new ReportsController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var actionResult = await controller.GetAnomalySummary("Mumbai Suburban");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var reportDto = Assert.IsType<DistrictAnomalyReportDto>(okResult.Value);
        Assert.Equal("Mumbai Suburban", reportDto.District);
    }
}

internal class ReportTestServiceProvider : IServiceProvider
{
    private readonly ApplicationDbContext _dbContext;

    public ReportTestServiceProvider(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IRequestHandler<GetAnomalySummaryByBoothQuery, DistrictAnomalyReportDto>))
        {
            return new GetAnomalySummaryByBoothQueryHandler(_dbContext);
        }
        return null;
    }
}
