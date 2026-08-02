using MatdarSathi.API.Domain.Common;

namespace MatdarSathi.API.Domain.Entities;

public class DistrictEscalationContact : BaseEntity
{
    public string District { get; set; } = null!;
    public string EroNameOffice { get; set; } = null!;
    public string DeoOfficeAddress { get; set; } = null!;
    public string HelplineNumber { get; set; } = null!;
    public string OfficialPortalUrl { get; set; } = null!;
}
