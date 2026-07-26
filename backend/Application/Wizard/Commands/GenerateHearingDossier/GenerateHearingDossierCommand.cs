using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Application.Wizard.Models;

namespace MatdanSathi.API.Application.Wizard.Commands.GenerateHearingDossier;

public record GenerateHearingDossierCommand(
    string VoterName,
    string EpicNumber,
    string AssemblyConstituency,
    string PollingStation,
    string AnomalyType,
    string CitizenshipEra,
    List<string> SelectedSelfProofs,
    List<string> SelectedParentProofs,
    string HearingBoothLocation) : IRequest<DossierResponseDto>;

public class GenerateHearingDossierCommandValidator : AbstractValidator<GenerateHearingDossierCommand>
{
    public GenerateHearingDossierCommandValidator()
    {
        RuleFor(v => v.VoterName)
            .NotEmpty().WithMessage("Voter Name is required.");

        RuleFor(v => v.EpicNumber)
            .NotEmpty().WithMessage("EPIC Number or Reference is required.");

        RuleFor(v => v.AnomalyType)
            .NotEmpty().WithMessage("Anomaly Type is required.");
    }
}

public class GenerateHearingDossierCommandHandler : IRequestHandler<GenerateHearingDossierCommand, DossierResponseDto>
{
    public Task<DossierResponseDto> Handle(GenerateHearingDossierCommand request, CancellationToken cancellationToken)
    {
        string refNo = "AERO-DOSSIER-" + Math.Abs(Guid.NewGuid().GetHashCode()).ToString().PadLeft(6, '0');
        string nowIso = DateTime.UtcNow.ToString("o");

        string applicableForm = request.AnomalyType switch
        {
            "Unmapped2002Record" => "Form 6 / Form 8 (Archival Tagging)",
            "ProgenyMismatch" => "Form 6 (Parentage Tagging)",
            "SurnameMarriageChange" => "Form 8 (Correction of Particulars)",
            "AgeDobConflict" => "Form 8 (DOB Correction)",
            "DoorLockedShifted" => "Form 8 (Shifting of Residence)",
            "Form7Objection" => "Form 7 (Objection / Deletion Notice)",
            _ => "Form 8 (Correction)"
        };

        string selfProofsStr = request.SelectedSelfProofs != null && request.SelectedSelfProofs.Count > 0
            ? string.Join(", ", request.SelectedSelfProofs)
            : "Aadhaar Card (Self)";

        string parentProofsStr = request.SelectedParentProofs != null && request.SelectedParentProofs.Count > 0
            ? string.Join(", ", request.SelectedParentProofs)
            : "N/A (Pre-1987 Era / Self Proof Only)";

        string boothLoc = string.IsNullOrWhiteSpace(request.HearingBoothLocation)
            ? "Electoral Registration Officer (AERO) Office / Local Polling Station Facility"
            : request.HearingBoothLocation;

        string noticeText = $"OFFICIAL AERO HEARING COVER SHEET SUMMARY\n" +
                            $"Dossier Ref: {refNo}\n" +
                            $"Voter: {request.VoterName} (EPIC: {request.EpicNumber})\n" +
                            $"Constituency: {request.AssemblyConstituency} | Booth: {request.PollingStation}\n" +
                            $"Anomaly Categorization: {request.AnomalyType} ({request.CitizenshipEra})\n" +
                            $"Prescribed Form: {applicableForm}\n" +
                            $"Attached Self Proofs: {selfProofsStr}\n" +
                            $"Attached Parent Proofs: {parentProofsStr}\n" +
                            $"Hearing Location: {boothLoc}\n" +
                            $"Notice Directive: Please present this dossier along with original document proofs to the Assistant Electoral Registration Officer (AERO) at the scheduled hearing camp.";

        var response = new DossierResponseDto(
            DossierReference: refNo,
            GeneratedAt: nowIso,
            VoterName: request.VoterName,
            EpicNumber: request.EpicNumber,
            AssemblyConstituency: string.IsNullOrWhiteSpace(request.AssemblyConstituency) ? "Constituency-1" : request.AssemblyConstituency,
            PollingStation: string.IsNullOrWhiteSpace(request.PollingStation) ? "Primary School Facility" : request.PollingStation,
            AnomalyType: request.AnomalyType,
            CitizenshipEra: request.CitizenshipEra ?? "Pre1987",
            ApplicableForm: applicableForm,
            SelectedSelfProofs: request.SelectedSelfProofs ?? new List<string> { "Aadhaar Card" },
            SelectedParentProofs: request.SelectedParentProofs ?? new List<string>(),
            HearingBoothLocation: boothLoc,
            HearingNoticeText: noticeText,
            IsReadyForPrint: true
        );

        return Task.FromResult(response);
    }
}
