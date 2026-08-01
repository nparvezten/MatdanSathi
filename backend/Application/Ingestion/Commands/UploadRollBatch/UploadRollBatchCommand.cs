using System;
using System.IO;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Enums;

namespace MatdarSathi.API.Application.Ingestion.Commands.UploadRollBatch;

public record UploadRollBatchCommand(
    string BoothId,
    string UploadedByVolunteerId,
    string SourceFileName,
    Stream FileStream
) : IRequest<RollIngestionBatchResponseDto>;

public record RollIngestionBatchResponseDto(
    Guid BatchId,
    string BoothId,
    string SourceFileName,
    IngestionStatus Status,
    DateTime UploadedAtUtc,
    string Message
);
