using ITIGraduationProject.Application.Features.ReportGeneratorChat.Commands.Models;
using ITIGraduationProject.Application.Features.ReportGeneratorChat.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("session")]
        public async Task<IActionResult> CreateSession()
        {
            var result = await _mediator.Send(
                new CreateReportChatSessionCommand());

            return Ok(result);
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(
            [FromBody] SendReportChatMessageCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{sessionId:guid}/history")]
        public async Task<IActionResult> GetHistory(
            Guid sessionId)
        {
            return Ok(
                await _mediator.Send(
                    new GetReportChatHistoryQuery(sessionId)));
        }
    }
}
