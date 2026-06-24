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
    [Authorize]
    [Route("api/[controller]")]
    public class AiChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Send Chat Message
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request)
        {
            // take the real UserId from the token provided by JwtService
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { message = "Token is missing user id claim." });

            var command = new SendChatMessageCommand
            {
                SessionId = request.SessionId,
                MessageText = request.MessageText,
                UserId = Guid.Parse(userIdString)
            };

            var response = await _mediator.Send(command);
            if (response.Succeeded)
                return Ok(response);

            return BadRequest(response);
        }
        #endregion

        #region Get Chat Session History
        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetChatHistory(Guid sessionId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { message = "Token is missing user id claim." });

            var userId = Guid.Parse(userIdString);

            var response = await _mediator.Send(
                new GetChatSessionHistoryQuery { SessionId = sessionId, RequestingUserId = userId });

            if (response.Succeeded)
                return Ok(response);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return Unauthorized(response);

            return NotFound(response);
        }
        #endregion
    }
    public class SendChatMessageRequest
    {
        public Guid SessionId { get; set; }
        public string MessageText { get; set; } = string.Empty;
    }
}