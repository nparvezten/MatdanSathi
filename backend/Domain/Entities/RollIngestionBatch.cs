using System;
using MatdarSathi.API.Domain.Common;
using MatdarSathi.API.Domain.Enums;

namespace MatdarSathi.API.Domain.Entities;

public class RollIngestionBatch : BaseEntity
{
    public string BoothId { get; set; } = null!;
    public string UploadedByVolunteerId { get; set; } = null!;
    public string SourceFileName { get; set; } = null!;
    public IngestionStatus IngestionStatus { get; set; } = IngestionStatus.Pending;
    public int RecordCount { get; set; } = 0;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public string? FailureReason { get; set; }
}
