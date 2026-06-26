using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;
using ITIGraduationProject.Application.Features.Profiles.Queries.Models;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        #region Mediator Injection To Send Request To Handler
        private readonly IMediator _mediator;
        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion

        #region Getting Profile Data By Email
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {

            var query = new GetProfileQuery { Email = email };
            var response = await _mediator.Send(query);

            if (response == null || !response.Succeeded) return NotFound(response);
            return Ok(response);
        }
        #endregion

        #region Update Profile Data
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileCommand command)
        {

            var response = await _mediator.Send(command);

            if (response != null && response.Succeeded)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        #endregion
    }
}