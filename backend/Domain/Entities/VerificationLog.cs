using System;
using MatdanSathi.API.Domain.Common;

namespace MatdanSathi.API.Domain.Entities;

public class VerificationLog : BaseEntity
{
    public Guid? VoterProfileId { get; set; }
    public string VerifierId { get; set; } = null!;
    public DateTimeOffset VerificationTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public bool IsVerified { get; set; }
    public string VerificationMethod { get; set; } = null!;
    public string? Notes { get; set; }

    // Navigation property
    public VoterProfile? VoterProfile { get; set; }
}
