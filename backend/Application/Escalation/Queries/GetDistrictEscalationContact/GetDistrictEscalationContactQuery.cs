using System;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MatdarSathi.API.Application.Escalation.Queries.GetDistrictEscalationContact;

public record DistrictEscalationContactDto
{
    public Guid Id { get; init; }
    public string District { get; init; } = null!;
    public string EroNameOffice { get; init; } = null!;
    public string DeoOfficeAddress { get; init; } = null!;
    public string HelplineNumber { get; init; } = null!;
    public string OfficialPortalUrl { get; init; } = null!;
}

public record GetDistrictEscalationContactQuery(string District) : IRequest<DistrictEscalationContactDto?>;

public class GetDistrictEscalationContactQueryHandler : IRequestHandler<GetDistrictEscalationContactQuery, DistrictEscalationContactDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDistrictEscalationContactQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DistrictEscalationContactDto?> Handle(GetDistrictEscalationContactQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.District))
        {
            return null;
        }

        var normalizedDistrict = request.District.Trim().ToLowerInvariant();

        var contact = await _context.DistrictEscalationContacts
            .FirstOrDefaultAsync(c => c.District.ToLower() == normalizedDistrict && !c.IsDeleted, cancellationToken);

        if (contact == null)
        {
            return null;
        }

        return new DistrictEscalationContactDto
        {
            Id = contact.Id,
            District = contact.District,
            EroNameOffice = contact.EroNameOffice,
            DeoOfficeAddress = contact.DeoOfficeAddress,
            HelplineNumber = contact.HelplineNumber,
            OfficialPortalUrl = contact.OfficialPortalUrl
        };
    }
}
