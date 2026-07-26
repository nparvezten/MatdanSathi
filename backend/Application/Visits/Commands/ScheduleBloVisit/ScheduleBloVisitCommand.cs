using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Domain.Entities;

namespace MatdarSathi.API.Application.Visits.Commands.ScheduleBloVisit;

public record ScheduleBloVisitCommand(
    string EpicNumber,
    string VoterName,
    string ContactNumber,
    string PhysicalNoticeSlipNumber,
    string PreferredDate,
    string PreferredTimeSlot,
    string HouseNo,
    string PollingStationName,
    string Notes) : IRequest<VisitSlipDto>;

public class ScheduleBloVisitCommandValidator : AbstractValidator<ScheduleBloVisitCommand>
{
    public ScheduleBloVisitCommandValidator()
    {
        RuleFor(v => v.EpicNumber)
            .NotEmpty().WithMessage("EPIC Card Number is required.")
            .MaximumLength(20).WithMessage("EPIC Number cannot exceed 20 characters.");

        RuleFor(v => v.VoterName)
            .NotEmpty().WithMessage("Voter Name is required.")
            .MaximumLength(150).WithMessage("Voter Name cannot exceed 150 characters.");

        RuleFor(v => v.PhysicalNoticeSlipNumber)
            .NotEmpty().WithMessage("Physical BLO Notice Slip Number is required.");

        RuleFor(v => v.PreferredDate)
            .NotEmpty().WithMessage("Preferred date is required.");

        RuleFor(v => v.PreferredTimeSlot)
            .NotEmpty().WithMessage("Preferred time slot is required.");
    }
}

public class ScheduleBloVisitCommandHandler : IRequestHandler<ScheduleBloVisitCommand, VisitSlipDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public ScheduleBloVisitCommandHandler(
        IApplicationDbContext context,
        ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    public async Task<VisitSlipDto> Handle(ScheduleBloVisitCommand request, CancellationToken cancellationToken)
    {
        var visitSlip = new VisitSlip
        {
            EpicNumber = request.EpicNumber,
            VoterName = request.VoterName,
            ContactNumber = request.ContactNumber,
            PhysicalNoticeSlipNumber = request.PhysicalNoticeSlipNumber,
            PreferredDate = request.PreferredDate,
            PreferredTimeSlot = request.PreferredTimeSlot,
            HouseNo = string.IsNullOrWhiteSpace(request.HouseNo) ? "N/A" : request.HouseNo,
            PollingStationName = string.IsNullOrWhiteSpace(request.PollingStationName) ? "Primary School Ward Facility" : request.PollingStationName,
            Notes = request.Notes ?? "Notice slip registered for follow-up visit.",
            Status = "Scheduled",
            AssignedBloName = "Ahmed Khan",
            BloContact = "1111122222"
        };

        _context.VisitSlips.Add(visitSlip);
        await _context.SaveChangesAsync(cancellationToken);

        return new VisitSlipDto
        {
            Id = visitSlip.Id,
            PhysicalNoticeSlipNumber = visitSlip.PhysicalNoticeSlipNumber,
            EpicNumber = request.EpicNumber,
            VoterName = request.VoterName,
            PreferredDate = visitSlip.PreferredDate,
            PreferredTimeSlot = visitSlip.PreferredTimeSlot,
            HouseNo = visitSlip.HouseNo,
            PollingStationName = visitSlip.PollingStationName,
            Status = visitSlip.Status,
            AssignedBloName = visitSlip.AssignedBloName,
            AssignedBloContact = "1111122222",
            ConfirmationMessage = $"Physical Notice Slip {visitSlip.PhysicalNoticeSlipNumber} registered successfully. Assigned BLO: {visitSlip.AssignedBloName}."
        };
    }
}
