using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.IdentityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator _mediatr;

        public IdentityController(IMediator mediatr)
        {
            _mediatr = mediatr;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterCommand requestDTO)
        {
            var Result =await _mediatr.Send(requestDTO);
            return Ok(Result);
        }
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command)
        {
            var result = await _mediatr.Send(command);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO command)
        {
            var result = await _mediatr.Send(command);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
