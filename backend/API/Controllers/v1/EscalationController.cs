using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Escalation.Queries.GetDistrictEscalationContact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/escalation")]
[AllowAnonymous]
public class EscalationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EscalationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{district}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DistrictEscalationContactDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEscalationContact([FromRoute] string district)
    {
        var query = new GetDistrictEscalationContactQuery(district);
        var result = await _mediator.Send(query, HttpContext.RequestAborted);

        if (result == null)
        {
            return NotFound(new { message = $"No district escalation contact directory entry found for '{district}'." });
        }

        return Ok(result);
    }
}
