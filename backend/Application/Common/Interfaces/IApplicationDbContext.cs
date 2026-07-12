using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdanSathi.API.Domain.Entities;

namespace MatdanSathi.API.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<VoterProfile> VoterProfiles { get; }
    DbSet<VerificationLog> VerificationLogs { get; }
    DbSet<BloCoordinateMap> BloCoordinateMaps { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
