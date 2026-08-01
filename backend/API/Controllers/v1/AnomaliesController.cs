using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Anomalies.Commands.SubmitLegacyAnomaly;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AnomaliesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnomaliesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Log certified historical extract anomaly and family household bundle.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitAnomaly([FromBody] SubmitLegacyAnomalyCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
