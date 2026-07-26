using System;
using MatdarSathi.API.Domain.Common;

namespace MatdarSathi.API.Domain.Entities;

public class UserVerifier : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string AssemblyConstituency { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Verifier"; // "SuperAdmin" or "Verifier"
    public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
    public DateTime? ApprovedAt { get; set; }
}
