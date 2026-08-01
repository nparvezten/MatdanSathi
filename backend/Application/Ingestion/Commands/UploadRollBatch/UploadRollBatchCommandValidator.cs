using FluentValidation;

namespace MatdarSathi.API.Application.Ingestion.Commands.UploadRollBatch;

public class UploadRollBatchCommandValidator : AbstractValidator<UploadRollBatchCommand>
{
    public UploadRollBatchCommandValidator()
    {
        RuleFor(x => x.BoothId)
            .NotEmpty().WithMessage("BoothId is required.")
            .MaximumLength(100);

        RuleFor(x => x.SourceFileName)
            .NotEmpty().WithMessage("SourceFileName is required.")
            .Must(name => name != null && name.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only PDF files are supported for roll ingestion.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File content stream is required.");
    }
}
