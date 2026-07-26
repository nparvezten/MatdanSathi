using System;

namespace MatdarSathi.API.Application.Visits.Commands.ScheduleBloVisit;

public record VisitSlipDto
{
    public Guid Id { get; init; }
    public string PhysicalNoticeSlipNumber { get; init; } = null!;
    public string EpicNumber { get; init; } = null!;
    public string VoterName { get; init; } = null!;
    public string PreferredDate { get; init; } = null!;
    public string PreferredTimeSlot { get; init; } = null!;
    public string HouseNo { get; init; } = null!;
    public string PollingStationName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string AssignedBloName { get; init; } = null!;
    public string AssignedBloContact { get; init; } = null!;
    public string ConfirmationMessage { get; init; } = null!;
}
