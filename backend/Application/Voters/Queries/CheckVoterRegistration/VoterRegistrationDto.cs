using System;

namespace MatdanSathi.API.Application.Voters.Queries.CheckVoterRegistration;

public record VoterRegistrationDto
{
    public Guid Id { get; init; }
    public string EpicNumber { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public int Age { get; init; }
    public string DateOfBirth { get; init; } = null!;
    public string Gender { get; init; } = null!;
    public string AssemblyConstituency { get; init; } = null!;
    public string PartNumber { get; init; } = null!;
    public string SectionNumber { get; init; } = null!;
    public int SerialNumber { get; init; }
    public string PollingStationName { get; init; } = null!;
    public string PollingStationLocation { get; init; } = null!;
    public string HouseNo { get; init; } = null!;
    public string BloName { get; init; } = null!;
    public string BloContact { get; init; } = null!;
    public bool IsVerified { get; init; }
}
