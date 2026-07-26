using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Application.Wizard.Commands.GenerateHearingDossier;
using MatdanSathi.API.Application.Wizard.Models;
using MatdanSathi.API.Application.Wizard.Queries.GetAnomalyGuidance;
using MatdanSathi.API.Application.Wizard.Queries.GetAnomalyRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    [HttpGet("guidance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GuidanceResponseDto))]
    public async Task<IActionResult> GetGuidance(
        [FromQuery] int age = 30,
        [FromQuery] int? birthYear = null,
        [FromQuery] string anomalyType = "SurnameMarriageChange")
    {
        var guidance = await _mediator.Send(new GetAnomalyGuidanceQuery(age, birthYear, anomalyType));
        return Ok(guidance);
    }

    [HttpPost("generate-hearing-dossier")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DossierResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateHearingDossier([FromBody] GenerateHearingDossierCommand command)
    {
        var dossier = await _mediator.Send(command);
        return Ok(dossier);
    }
}
