using ITIGraduationProject.Application.Features.Admin.UserControls.Commands.Mdels;
using ITIGraduationProject.Application.Features.Admin.UserControls.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin
{
   
        [ApiController]
        [Route("api/admin/users")]
        [Authorize(Roles = Roles.Admin)]
        public class AdminUsersController : ControllerBase
        {
            private readonly IMediator _mediator;

            public AdminUsersController(IMediator mediator)
            {
                _mediator = mediator;
            }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("{id}")]
            public async Task<IActionResult> GetUserById(Guid id)
            {
                var result = await _mediator.Send(new GetUserByIdQuery { Id = id });
                return StatusCode((int)result.StatusCode, result);
            }

            [HttpPost("invite")]
            public async Task<IActionResult> InviteUser(InviteUserCommand command)
            {
                var result = await _mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateUser(Guid id, UpdateUserCommand command)
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }

            [HttpPatch("{id}/status")]
            public async Task<IActionResult> ChangeStatus(Guid id, ChangeUserStatusCommand command)
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return StatusCode((int)result.StatusCode, result);
            }
        }
    
}
