using System;
using System.ComponentModel.DataAnnotations.Schema;
using MatdarSathi.API.Domain.Common;
using MatdarSathi.API.Domain.Enums;

namespace MatdarSathi.API.Domain.Entities;

public class ObjectionCase : BaseEntity
{
    public ObjectionCaseType CaseType { get; set; }
    public ObjectionStatus Status { get; set; } = ObjectionStatus.Draft;
    public Guid? LinkedVoterProfileId { get; set; }

    public DateTimeOffset SubmittedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastStatusUpdateUtc { get; set; } = DateTimeOffset.UtcNow;

    // Encrypted PII & ERO Note fields (AES-256)
    public string? EroNotesEncrypted { get; set; }
    public string? ApplicantNameEncrypted { get; set; }
    public string? EpicNumberEncrypted { get; set; }
    public string? EpicNumberBlindIndex { get; set; }

    // Unmapped properties for cleartext payload processing
    [NotMapped]
    public string? EroNotes { get; set; }

    [NotMapped]
    public string? ApplicantName { get; set; }

    [NotMapped]
    public string? EpicNumber { get; set; }

    // Navigation Property (Optional reference to VoterProfile)
    public VoterProfile? LinkedVoterProfile { get; set; }
}
