using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdarSathi.API.Application.Ingestion.Commands.UploadRollBatch;
using MatdarSathi.API.Domain.Enums;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace MatdarSathi.API.Tests;

public class IngestionTests
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
    public async Task UploadRollBatchCommand_ValidPdf_PersistsPendingBatchRecord()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new UploadRollBatchCommandHandler(dbContext);

        var dummyPdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4 Mock Draft Roll Content");
        using var stream = new MemoryStream(dummyPdfBytes);

        var command = new UploadRollBatchCommand(
            BoothId: "BOOTH-182-A",
            UploadedByVolunteerId: "vol-user-101",
            SourceFileName: "booth_182_draft_roll.pdf",
            FileStream: stream
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("BOOTH-182-A", result.BoothId);
        Assert.Equal("booth_182_draft_roll.pdf", result.SourceFileName);
        Assert.Equal(IngestionStatus.Pending, result.Status);

        var savedBatch = await dbContext.RollIngestionBatches.FirstOrDefaultAsync(b => b.Id == result.BatchId);
        Assert.NotNull(savedBatch);
        Assert.Equal(IngestionStatus.Pending, savedBatch.IngestionStatus);
    }

    [Fact]
    public void UploadRollBatchCommandValidator_NonPdfFile_FailsValidation()
    {
        // Arrange
        var validator = new UploadRollBatchCommandValidator();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));

        var command = new UploadRollBatchCommand(
            BoothId: "BOOTH-101",
            UploadedByVolunteerId: "vol-1",
            SourceFileName: "invalid_file.txt",
            FileStream: stream
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SourceFileName");
    }
}
