using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Ingestion.Commands.UploadRollBatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatdarSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class IngestionController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngestionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a booth-wise PDF draft roll for Watchdog comparison.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RollIngestionBatchResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBatch([FromForm] IFormFile file, [FromForm] string boothId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "PDF file is required for roll ingestion." });
        }

        var volunteerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "volunteer-anon";

        using var stream = file.OpenReadStream();
        var command = new UploadRollBatchCommand(
            BoothId: boothId,
            UploadedByVolunteerId: volunteerId,
            SourceFileName: file.FileName,
            FileStream: stream
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve booth assignment status list for volunteer task claiming.
    /// </summary>
    [HttpGet("booths")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooths([FromQuery] string? assembly)
    {
        var query = new MatdarSathi.API.Application.Ingestion.Queries.GetBoothAssignments.GetBoothAssignmentsQuery(assembly);
        var result = await _mediator.Send(query, HttpContext.RequestAborted);
        return Ok(result);
    }

    /// <summary>
    /// Claim a booth assignment to prevent duplicate ingestion uploads.
    /// </summary>
    [HttpPost("booths/{boothId}/claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClaimBooth([FromRoute] string boothId)
    {
        var volunteerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "volunteer-anon";
        var command = new MatdarSathi.API.Application.Ingestion.Commands.ClaimBooth.ClaimBoothCommand(boothId, volunteerId);
        var result = await _mediator.Send(command, HttpContext.RequestAborted);
        return Ok(result);
    }
}
