using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MatdarSathi.API.Application.Common.Constants;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Domain.Enums;

namespace MatdarSathi.API.Application.Objections.Commands.CreateObjectionCase;

public record ObjectionCaseDto
{
    public Guid Id { get; init; }
    public ObjectionCaseType CaseType { get; init; }
    public ObjectionStatus Status { get; init; }
    public Guid? LinkedVoterProfileId { get; init; }
    public DateTimeOffset SubmittedAtUtc { get; init; }
    public DateTimeOffset LastStatusUpdateUtc { get; init; }
    public string? ApplicantName { get; init; }
    public string? EpicNumber { get; init; }
    public string? EroNotes { get; init; }
}

public record CreateObjectionCaseCommand(
    ObjectionCaseType CaseType,
    string ApplicantName,
    string EpicNumber,
    Guid? LinkedVoterProfileId = null,
    string? InitialNotes = null) : IRequest<ObjectionCaseDto>;

public class CreateObjectionCaseCommandValidator : AbstractValidator<CreateObjectionCaseCommand>
{
    public CreateObjectionCaseCommandValidator()
    {
        RuleFor(x => x.ApplicantName)
            .NotEmpty().WithMessage("Applicant name is required.")
            .MaximumLength(200).WithMessage("Applicant name must not exceed 200 characters.");

        RuleFor(x => x.EpicNumber)
            .NotEmpty().WithMessage("EPIC card number is required.")
            .Matches(EpicRegexConstants.EpicPattern).WithMessage("EPIC number format is invalid.");
    }
}

public class CreateObjectionCaseCommandHandler : IRequestHandler<CreateObjectionCaseCommand, ObjectionCaseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateObjectionCaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ObjectionCaseDto> Handle(CreateObjectionCaseCommand request, CancellationToken cancellationToken)
    {
        var objectionCase = new ObjectionCase
        {
            CaseType = request.CaseType,
            Status = ObjectionStatus.Filed,
            LinkedVoterProfileId = request.LinkedVoterProfileId,
            SubmittedAtUtc = DateTimeOffset.UtcNow,
            LastStatusUpdateUtc = DateTimeOffset.UtcNow,
            ApplicantName = request.ApplicantName,
            EpicNumber = request.EpicNumber,
            EroNotes = request.InitialNotes
        };

        _context.ObjectionCases.Add(objectionCase);
        await _context.SaveChangesAsync(cancellationToken);

        return new ObjectionCaseDto
        {
            Id = objectionCase.Id,
            CaseType = objectionCase.CaseType,
            Status = objectionCase.Status,
            LinkedVoterProfileId = objectionCase.LinkedVoterProfileId,
            SubmittedAtUtc = objectionCase.SubmittedAtUtc,
            LastStatusUpdateUtc = objectionCase.LastStatusUpdateUtc,
            ApplicantName = request.ApplicantName,
            EpicNumber = request.EpicNumber,
            EroNotes = request.InitialNotes
        };
    }
}
