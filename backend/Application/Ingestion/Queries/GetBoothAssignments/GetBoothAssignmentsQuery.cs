using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MatdarSathi.API.Application.Ingestion.Queries.GetBoothAssignments;

public record BoothAssignmentStatusDto(
    string BoothId,
    string BoothName,
    string AssemblyConstituency,
    bool IsClaimed,
    string? ClaimedByVolunteerId,
    DateTimeOffset? ClaimedAtUtc);

public record GetBoothAssignmentsQuery(string? AssemblyConstituency = null) : IRequest<List<BoothAssignmentStatusDto>>;

public class GetBoothAssignmentsQueryHandler : IRequestHandler<GetBoothAssignmentsQuery, List<BoothAssignmentStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBoothAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BoothAssignmentStatusDto>> Handle(GetBoothAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var activeAssignments = await _context.VolunteerBoothAssignments
            .Where(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var assignmentDict = activeAssignments
            .GroupBy(a => a.BoothId)
            .ToDictionary(g => g.Key, g => g.First());

        var defaultBooths = new[]
        {
            new { BoothId = "BOOTH-101-WEST", BoothName = "Primary School Building Room 1", Assembly = "182-Mumbai" },
            new { BoothId = "BOOTH-102-EAST", BoothName = "BMC Secondary School Hall", Assembly = "182-Mumbai" },
            new { BoothId = "BOOTH-103-NORTH", BoothName = "Government Polytechnic Room 4", Assembly = "182-Mumbai" },
            new { BoothId = "BOOTH-104-SOUTH", BoothName = "Community Hall West Wing", Assembly = "182-Mumbai" }
        };

        var result = new List<BoothAssignmentStatusDto>();

        foreach (var b in defaultBooths)
        {
            bool isClaimed = assignmentDict.TryGetValue(b.BoothId, out var assignment);
            result.Add(new BoothAssignmentStatusDto(
                BoothId: b.BoothId,
                BoothName: b.BoothName,
                AssemblyConstituency: b.Assembly,
                IsClaimed: isClaimed,
                ClaimedByVolunteerId: assignment?.VolunteerId,
                ClaimedAtUtc: assignment?.ClaimedAtUtc
            ));
        }

        return result;
    }
}
