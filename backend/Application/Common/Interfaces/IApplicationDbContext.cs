using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdarSathi.API.Domain.Entities;

namespace MatdarSathi.API.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<VoterProfile> VoterProfiles { get; }
    DbSet<VerificationLog> VerificationLogs { get; }
    DbSet<BloCoordinateMap> BloCoordinateMaps { get; }
    DbSet<VisitSlip> VisitSlips { get; }
    DbSet<UserVerifier> UserVerifiers { get; }
    DbSet<LegacyAnomalyRecord> LegacyAnomalyRecords { get; }
    DbSet<RollIngestionBatch> RollIngestionBatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
