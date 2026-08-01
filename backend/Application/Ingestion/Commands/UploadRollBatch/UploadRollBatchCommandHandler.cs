using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Domain.Enums;

namespace MatdarSathi.API.Application.Ingestion.Commands.UploadRollBatch;

public class UploadRollBatchCommandHandler : IRequestHandler<UploadRollBatchCommand, RollIngestionBatchResponseDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UploadRollBatchCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RollIngestionBatchResponseDto> Handle(UploadRollBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = new RollIngestionBatch
        {
            BoothId = request.BoothId.Trim(),
            UploadedByVolunteerId = string.IsNullOrWhiteSpace(request.UploadedByVolunteerId) ? "system-volunteer" : request.UploadedByVolunteerId.Trim(),
            SourceFileName = request.SourceFileName.Trim(),
            IngestionStatus = IngestionStatus.Pending,
            RecordCount = 0,
            UploadedAtUtc = DateTime.UtcNow
        };

        // Persist local file copy inside UploadedRolls directory
        var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedRolls");
        Directory.CreateDirectory(baseDir);
        var storedFilePath = Path.Combine(baseDir, $"{batch.Id}_{request.SourceFileName}");

        using (var destStream = new FileStream(storedFilePath, FileMode.Create, FileAccess.Write))
        {
            await request.FileStream.CopyToAsync(destStream, cancellationToken);
        }

        _dbContext.RollIngestionBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RollIngestionBatchResponseDto(
            batch.Id,
            batch.BoothId,
            batch.SourceFileName,
            batch.IngestionStatus,
            batch.UploadedAtUtc,
            "Booth draft roll uploaded successfully and queued for background Watchdog ingestion."
        );
    }
}
