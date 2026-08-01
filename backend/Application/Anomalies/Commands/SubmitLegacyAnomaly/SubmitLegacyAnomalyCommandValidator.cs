using FluentValidation;

namespace MatdarSathi.API.Application.Anomalies.Commands.SubmitLegacyAnomaly;

public class SubmitLegacyAnomalyCommandValidator : AbstractValidator<SubmitLegacyAnomalyCommand>
{
    public SubmitLegacyAnomalyCommandValidator()
    {
        RuleFor(v => v.ReceiptNumber)
            .NotEmpty().WithMessage("Certified extract receipt number is required.")
            .MaximumLength(100);

        RuleFor(v => v.DeceasedName)
            .NotEmpty().WithMessage("Deceased elector name is required.")
            .MaximumLength(200);

        RuleFor(v => v.YearOfDeath)
            .InclusiveBetween(1900, 2026).WithMessage("Year of death must be between 1900 and 2026.");

        RuleFor(v => v.DeathCertRegNo)
            .NotEmpty().WithMessage("Death certificate registration number is required.")
            .MaximumLength(100);

        RuleFor(v => v.ConstituencyName)
            .NotEmpty().WithMessage("Constituency name or number is required.")
            .MaximumLength(200);

        RuleFor(v => v.PartNumber)
            .NotEmpty().WithMessage("Part number is required.");

        RuleFor(v => v.PageNumber)
            .NotEmpty().WithMessage("Page number is required.");

        RuleFor(v => v.SerialRange)
            .NotEmpty().WithMessage("Serial range is required.");

        RuleFor(v => v.FamilyMembers)
            .NotNull().WithMessage("Family members list must be provided.");
    }
}
