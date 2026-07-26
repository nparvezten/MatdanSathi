using System;
using System.ComponentModel.DataAnnotations.Schema;
using MatdarSathi.API.Domain.Common;

namespace MatdarSathi.API.Domain.Entities;

public class VisitSlip : BaseEntity
{
    // Deterministic Blind Index for exact-match searches on EPIC Card Number
    public string EpicNumberBlindIndex { get; set; } = null!;

    // Encrypted PII Fields (AES-256)
    public string EpicNumberEncrypted { get; set; } = null!;
    public string VoterNameEncrypted { get; set; } = null!;
    public string ContactNumberEncrypted { get; set; } = null!;
    public string BloContactEncrypted { get; set; } = null!;

    // Unmapped properties for plain text ingestion / ChangeTracker handling
    [NotMapped]
    public string? EpicNumber { get; set; }

    [NotMapped]
    public string? VoterName { get; set; }

    [NotMapped]
    public string? ContactNumber { get; set; }

    [NotMapped]
    public string? BloContact { get; set; }

    // Physical Notice & Visit Details
    public string PhysicalNoticeSlipNumber { get; set; } = null!;
    public string PreferredDate { get; set; } = null!;
    public string PreferredTimeSlot { get; set; } = null!;
    public string HouseNo { get; set; } = null!;
    public string PollingStationName { get; set; } = null!;
    public string Notes { get; set; } = null!;

    // Status Tracking
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled
    public string AssignedBloName { get; set; } = null!;
}
