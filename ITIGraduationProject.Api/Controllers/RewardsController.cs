using ITIGraduationProject.Application.Features.Rewards.Commands.Models;
using ITIGraduationProject.Application.Features.Rewards.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITIGraduationProject.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RewardsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RewardsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRewards()
        {
            var response = await _mediator.Send(new CalculateRewardsCommand());
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRewards(System.Guid userId)
        {
            var response = await _mediator.Send(new GetUserRewardsQuery { UserId = userId });
            return Ok(response);
        }
    }
}