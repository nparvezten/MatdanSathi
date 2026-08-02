using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Reports.Queries.GetAnomalySummaryByBooth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[AllowAnonymous]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("anomaly-summary")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DistrictAnomalyReportDto))]
    public async Task<IActionResult> GetAnomalySummary([FromQuery] string? district)
    {
        var query = new GetAnomalySummaryByBoothQuery(district);
        var result = await _mediator.Send(query, HttpContext.RequestAborted);
        return Ok(result);
    }
}
