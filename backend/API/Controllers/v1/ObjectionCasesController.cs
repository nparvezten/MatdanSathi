using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Objections.Commands.CreateObjectionCase;
using MatdarSathi.API.Application.Objections.Commands.UpdateObjectionCaseStatus;
using MatdarSathi.API.Application.Objections.Queries.GetObjectionCaseById;
using MatdarSathi.API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatdarSathi.API.Controllers.v1;

public record UpdateStatusRequestDto(ObjectionStatus NewStatus, string? EroNotes = null);

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/objections")]
[Authorize]
public class ObjectionCasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ObjectionCasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ObjectionCaseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateObjection([FromBody] CreateObjectionCaseCommand command)
    {
        var result = await _mediator.Send(command, HttpContext.RequestAborted);
        return CreatedAtAction(nameof(GetObjectionById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObjectionCaseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequestDto dto)
    {
        var command = new UpdateObjectionCaseStatusCommand(id, dto.NewStatus, dto.EroNotes);
        var result = await _mediator.Send(command, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ObjectionCaseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetObjectionById([FromRoute] Guid id)
    {
        var query = new GetObjectionCaseByIdQuery(id);
        var result = await _mediator.Send(query, HttpContext.RequestAborted);
        return Ok(result);
    }
}
