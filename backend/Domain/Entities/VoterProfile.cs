using System;
using System.ComponentModel.DataAnnotations.Schema;
using MatdanSathi.API.Domain.Common;

namespace MatdanSathi.API.Domain.Entities;

public class VoterProfile : BaseEntity
{
    // Deterministic Blind Index for exact-match searches on EPIC Card Number
    public string EpicNumberBlindIndex { get; set; } = null!;

    // Encrypted PII Fields (AES-256)
    public string EpicNumberEncrypted { get; set; } = null!;
    public string FullNameEncrypted { get; set; } = null!;
    public string DateOfBirthEncrypted { get; set; } = null!;
    public string BloContactEncrypted { get; set; } = null!;

    // Unmapped properties for plain text ingestion
    [NotMapped]
    public string? EpicNumber { get; set; }

    [NotMapped]
    public string? FullName { get; set; }

    [NotMapped]
    public string? DateOfBirth { get; set; }

    [NotMapped]
    public string? BloContact { get; set; }

    // Non-PII Public/General Fields
    public int Age { get; set; }
    public string Gender { get; set; } = null!;
    public string AssemblyConstituency { get; set; } = null!;
    public string PartNumber { get; set; } = null!;
    public string SectionNumber { get; set; } = null!;
    public int SerialNumber { get; set; }
    public string PollingStationName { get; set; } = null!;
    public string PollingStationLocation { get; set; } = null!;
    public string HouseNo { get; set; } = null!;

    public string BloName { get; set; } = null!;
}
