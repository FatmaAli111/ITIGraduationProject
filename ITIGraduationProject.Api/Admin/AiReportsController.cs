using ITIGraduationProject.Application.Features.Admin.AiReports.Commands.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin;

[ApiController]
[Route("api/admin/ai-reports")]
[Authorize(Roles = Roles.Admin)]
public sealed class AiReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiReportsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateAiReportCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}
