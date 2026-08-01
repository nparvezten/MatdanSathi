using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;

namespace MatdarSathi.API.Application.Anomalies.Commands.SubmitLegacyAnomaly;

public class SubmitLegacyAnomalyCommandHandler : IRequestHandler<SubmitLegacyAnomalyCommand, LegacyAnomalyResponseDto>
{
    private readonly IApplicationDbContext _dbContext;

    public SubmitLegacyAnomalyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LegacyAnomalyResponseDto> Handle(SubmitLegacyAnomalyCommand request, CancellationToken cancellationToken)
    {
        var familyBundleJson = JsonSerializer.Serialize(request.FamilyMembers);

        var entity = new LegacyAnomalyRecord
        {
            ReceiptNumber = request.ReceiptNumber.Trim(),
            DeceasedName = request.DeceasedName.Trim(),
            YearOfDeath = request.YearOfDeath,
            DeathCertRegNo = request.DeathCertRegNo.Trim(),
            ConstituencyName = request.ConstituencyName.Trim(),
            PartNumber = request.PartNumber.Trim(),
            PageNumber = request.PageNumber.Trim(),
            SerialRange = request.SerialRange.Trim(),
            FamilyBundleJson = familyBundleJson
        };

        _dbContext.LegacyAnomalyRecords.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LegacyAnomalyResponseDto(
            entity.Id,
            entity.ReceiptNumber,
            "LoggedSuccessfully",
            "Certified extract anomaly and family household bundle saved securely.",
            DateTime.UtcNow
        );
    }
}
