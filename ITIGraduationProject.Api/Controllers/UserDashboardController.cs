using ITIGraduationProject.Application.Features.UserDashboard.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/user-dashboard")]
    public class UserDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserDashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet("me")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            return await GetDashboard(userId);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetDashboard(Guid userId)
        {
            var response = await _mediator.Send(new GetUserDashboardQuery(userId));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(response);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
