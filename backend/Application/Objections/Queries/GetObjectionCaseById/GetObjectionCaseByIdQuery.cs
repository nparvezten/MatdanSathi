using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Objections.Commands.CreateObjectionCase;
using Microsoft.EntityFrameworkCore;

namespace MatdarSathi.API.Application.Objections.Queries.GetObjectionCaseById;

public record GetObjectionCaseByIdQuery(Guid Id) : IRequest<ObjectionCaseDto>;

public class GetObjectionCaseByIdQueryHandler : IRequestHandler<GetObjectionCaseByIdQuery, ObjectionCaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public GetObjectionCaseByIdQueryHandler(
        IApplicationDbContext context,
        ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    public async Task<ObjectionCaseDto> Handle(GetObjectionCaseByIdQuery request, CancellationToken cancellationToken)
    {
        var objectionCase = await _context.ObjectionCases
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (objectionCase == null)
        {
            throw new KeyNotFoundException($"Objection case with ID '{request.Id}' was not found.");
        }

        string? decryptedApplicantName = !string.IsNullOrEmpty(objectionCase.ApplicantNameEncrypted)
            ? _cryptographyService.Decrypt(objectionCase.ApplicantNameEncrypted)
            : objectionCase.ApplicantName;

        string? decryptedEpicNumber = !string.IsNullOrEmpty(objectionCase.EpicNumberEncrypted)
            ? _cryptographyService.Decrypt(objectionCase.EpicNumberEncrypted)
            : objectionCase.EpicNumber;

        string? decryptedEroNotes = !string.IsNullOrEmpty(objectionCase.EroNotesEncrypted)
            ? _cryptographyService.Decrypt(objectionCase.EroNotesEncrypted)
            : objectionCase.EroNotes;

        return new ObjectionCaseDto
        {
            Id = objectionCase.Id,
            CaseType = objectionCase.CaseType,
            Status = objectionCase.Status,
            LinkedVoterProfileId = objectionCase.LinkedVoterProfileId,
            SubmittedAtUtc = objectionCase.SubmittedAtUtc,
            LastStatusUpdateUtc = objectionCase.LastStatusUpdateUtc,
            ApplicantName = decryptedApplicantName,
            EpicNumber = decryptedEpicNumber,
            EroNotes = decryptedEroNotes
        };
    }
}
