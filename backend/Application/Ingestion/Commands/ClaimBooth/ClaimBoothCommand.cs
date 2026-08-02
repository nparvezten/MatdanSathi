using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Ingestion.Queries.GetBoothAssignments;
using MatdarSathi.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatdarSathi.API.Application.Ingestion.Commands.ClaimBooth;

public record ClaimBoothCommand(
    string BoothId,
    string VolunteerId,
    string? BoothName = null,
    string? AssemblyConstituency = null) : IRequest<BoothAssignmentStatusDto>;

public class ClaimBoothCommandValidator : AbstractValidator<ClaimBoothCommand>
{
    public ClaimBoothCommandValidator()
    {
        RuleFor(x => x.BoothId).NotEmpty().WithMessage("Booth ID is required.");
        RuleFor(x => x.VolunteerId).NotEmpty().WithMessage("Volunteer ID is required.");
    }
}

public class ClaimBoothCommandHandler : IRequestHandler<ClaimBoothCommand, BoothAssignmentStatusDto>
{
    private readonly IApplicationDbContext _context;

    public ClaimBoothCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BoothAssignmentStatusDto> Handle(ClaimBoothCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.VolunteerBoothAssignments
            .FirstOrDefaultAsync(a => a.BoothId == request.BoothId && !a.IsDeleted, cancellationToken);

        if (existing != null)
        {
            if (existing.VolunteerId != request.VolunteerId)
            {
                throw new InvalidOperationException($"Booth '{request.BoothId}' has already been claimed by volunteer '{existing.VolunteerId}'.");
            }

            return new BoothAssignmentStatusDto(
                BoothId: existing.BoothId,
                BoothName: existing.BoothName,
                AssemblyConstituency: existing.AssemblyConstituency,
                IsClaimed: true,
                ClaimedByVolunteerId: existing.VolunteerId,
                ClaimedAtUtc: existing.ClaimedAtUtc
            );
        }

        var assignment = new VolunteerBoothAssignment
        {
            BoothId = request.BoothId,
            BoothName = request.BoothName ?? "Polling Station",
            AssemblyConstituency = request.AssemblyConstituency ?? "Constituency-1",
            VolunteerId = request.VolunteerId,
            ClaimedAtUtc = DateTimeOffset.UtcNow
        };

        _context.VolunteerBoothAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return new BoothAssignmentStatusDto(
            BoothId: assignment.BoothId,
            BoothName: assignment.BoothName,
            AssemblyConstituency: assignment.AssemblyConstituency,
            IsClaimed: true,
            ClaimedByVolunteerId: assignment.VolunteerId,
            ClaimedAtUtc: assignment.ClaimedAtUtc
        );
    }
}
