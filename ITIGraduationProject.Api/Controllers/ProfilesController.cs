using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;
using ITIGraduationProject.Application.Features.Profiles.Queries.Models;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized("User is not authorized or token is invalid.");
            }

            var response = await _mediator.Send(new GetProfileQuery(email));

            if (response != null && response.Succeeded)
            {
                return Ok(response);
            }

            return NotFound(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileCommand command)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not authorized or token is invalid.");
            }

            command.UserId = currentUserId;

            var response = await _mediator.Send(command);

            if (response != null && response.Succeeded)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}