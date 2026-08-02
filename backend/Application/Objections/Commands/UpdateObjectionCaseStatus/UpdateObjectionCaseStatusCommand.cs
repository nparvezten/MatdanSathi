using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Objections.Commands.CreateObjectionCase;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatdarSathi.API.Application.Objections.Commands.UpdateObjectionCaseStatus;

public record UpdateObjectionCaseStatusCommand(
    Guid Id,
    ObjectionStatus NewStatus,
    string? EroNotes = null) : IRequest<ObjectionCaseDto>;

public class UpdateObjectionCaseStatusCommandValidator : AbstractValidator<UpdateObjectionCaseStatusCommand>
{
    public UpdateObjectionCaseStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Objection case ID is required.");
    }
}

public class UpdateObjectionCaseStatusCommandHandler : IRequestHandler<UpdateObjectionCaseStatusCommand, ObjectionCaseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public UpdateObjectionCaseStatusCommandHandler(
        IApplicationDbContext context,
        ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    public async Task<ObjectionCaseDto> Handle(UpdateObjectionCaseStatusCommand request, CancellationToken cancellationToken)
    {
        var objectionCase = await _context.ObjectionCases
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (objectionCase == null)
        {
            throw new KeyNotFoundException($"Objection case with ID '{request.Id}' was not found.");
        }

        // Validate workflow status transition lifecycle rules
        ValidateStatusTransition(objectionCase.Status, request.NewStatus);

        objectionCase.Status = request.NewStatus;
        objectionCase.LastStatusUpdateUtc = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.EroNotes))
        {
            objectionCase.EroNotes = request.EroNotes;
        }

        await _context.SaveChangesAsync(cancellationToken);

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

    public static void ValidateStatusTransition(ObjectionStatus currentStatus, ObjectionStatus newStatus)
    {
        if (currentStatus == newStatus)
        {
            return;
        }

        bool isValid = currentStatus switch
        {
            ObjectionStatus.Draft => newStatus == ObjectionStatus.Filed,
            ObjectionStatus.Filed => newStatus is ObjectionStatus.Acknowledged or ObjectionStatus.UnderReview or ObjectionStatus.Rejected,
            ObjectionStatus.Acknowledged => newStatus is ObjectionStatus.UnderReview or ObjectionStatus.Resolved or ObjectionStatus.Rejected,
            ObjectionStatus.UnderReview => newStatus is ObjectionStatus.Resolved or ObjectionStatus.Rejected,
            ObjectionStatus.Resolved => false,
            ObjectionStatus.Rejected => false,
            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid status transition from '{currentStatus}' to '{newStatus}'.");
        }
    }
}
