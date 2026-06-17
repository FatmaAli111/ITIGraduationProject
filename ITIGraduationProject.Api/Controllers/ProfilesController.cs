using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;

using System.Net;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        #region Mediator Injection To Send Request To Handler

        private readonly IMediator _mediator;
        public ProfilesController(IMediator mediator) {
            _mediator = mediator;
        }
        #endregion

        #region Getting Profile Data By User Id
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile([FromQuery] string userId) {

            var query = new GetProfileQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

        #region Update Profile Data By User Id
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileCommand command) {

            var response = await _mediator.Send(command);

            if (response.StatusCode == HttpStatusCode.OK) {
                return Ok(response);
            }
            if (response.StatusCode == HttpStatusCode.NotFound) {
                return NotFound(response);
            }

            return BadRequest(response);
        }
        // used [FromForm] because using IFormFile for Uploading the Profile Image
        #endregion

    }
}
