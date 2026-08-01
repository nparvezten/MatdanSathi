using System;
using System.ComponentModel.DataAnnotations.Schema;
using MatdarSathi.API.Domain.Common;

namespace MatdarSathi.API.Domain.Entities;

public class LegacyAnomalyRecord : BaseEntity
{
    public string ReceiptNumber { get; set; } = null!;

    // Encrypted & Blind Index fields for Deceased Elector
    public string DeceasedNameBlindIndex { get; set; } = null!;
    public string DeceasedNameEncrypted { get; set; } = null!;
    public int YearOfDeath { get; set; }
    public string DeathCertRegNoEncrypted { get; set; } = null!;

    // Historical Roll Details
    public string ConstituencyName { get; set; } = null!;
    public string PartNumber { get; set; } = null!;
    public string PageNumber { get; set; } = null!;
    public string SerialRange { get; set; } = null!;

    // Encrypted Family Household Bundle (AES-256 JSON)
    public string FamilyBundleJsonEncrypted { get; set; } = null!;

    [NotMapped]
    public string? DeceasedName { get; set; }

    [NotMapped]
    public string? DeathCertRegNo { get; set; }

    [NotMapped]
    public string? FamilyBundleJson { get; set; }
}
