using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MatdarSathi.API.Application.Voters.Queries.CheckVoterRegistration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("strict-limit")]
public class VotersController : ControllerBase
{
    private readonly IMediator _mediator;

    public VotersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("check")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VoterRegistrationDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckRegistration([FromBody] CheckVoterRegistrationQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
