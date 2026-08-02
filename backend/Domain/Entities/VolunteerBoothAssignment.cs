using System;
using MatdarSathi.API.Domain.Common;

namespace MatdarSathi.API.Domain.Entities;

public class VolunteerBoothAssignment : BaseEntity
{
    public string BoothId { get; set; } = null!;
    public string BoothName { get; set; } = null!;
    public string AssemblyConstituency { get; set; } = null!;
    public string VolunteerId { get; set; } = null!;
    public DateTimeOffset ClaimedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
