using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MatdanSathi.API.Application.Wizard.Queries.GetAnomalyRules;

namespace MatdanSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("strict-limit")]
public class WizardController : ControllerBase
{
    private readonly IMediator _mediator;

    public WizardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("anomaly-rules")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AnomalyRuleDto>))]
    public async Task<IActionResult> GetAnomalyRules([FromQuery] string? anomalyType)
    {
        var rules = await _mediator.Send(new GetAnomalyRulesQuery(anomalyType));
        return Ok(rules);
    }
}
