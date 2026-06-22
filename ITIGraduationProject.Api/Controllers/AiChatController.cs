using ITIGraduationProject.Application.Features.AiChat.Commands.Models;
using ITIGraduationProject.Application.Features.AiChat.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    public class AiChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request)
        {
            // take the real UserId from the token provided by JwtService
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var command = new SendChatMessageCommand
            {
                SessionId = request.SessionId,
                MessageText = request.MessageText,
                UserId = Guid.Parse(userIdString)
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetChatHistory(Guid sessionId)
        {
            var response = await _mediator.Send(new GetChatSessionHistoryQuery { SessionId = sessionId });
            return Ok(response);
        }
    }
    public class SendChatMessageRequest
    {
        public Guid SessionId { get; set; }
        public string MessageText { get; set; } = string.Empty;
    }
}