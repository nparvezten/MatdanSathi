using FluentValidation;

namespace MatdanSathi.API.Application.Voters.Queries.CheckVoterRegistration;

public class CheckVoterRegistrationQueryValidator : AbstractValidator<CheckVoterRegistrationQuery>
{
    public CheckVoterRegistrationQueryValidator()
    {
        RuleFor(x => x.EpicNumber)
            .NotEmpty().WithMessage("EPIC number is required.")
            .MinimumLength(5).WithMessage("EPIC number is too short.")
            .MaximumLength(20).WithMessage("EPIC number must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9/\-]+$").WithMessage("EPIC number must contain only alphanumeric characters, slashes, or hyphens.");

        RuleFor(x => x.VerifierId)
            .NotEmpty().WithMessage("Verifier ID is required.");

        RuleFor(x => x.VerificationMethod)
            .NotEmpty().WithMessage("Verification method is required.")
            .MaximumLength(50).WithMessage("Verification method must not exceed 50 characters.");
    }
}
