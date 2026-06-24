using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [AllowAnonymous]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
            => Ok(await _mediator.Send(new GetDashboardOverviewQuery()));

        [HttpGet("orders-by-status")]
        public async Task<IActionResult> GetOrdersByStatus()
            => Ok(await _mediator.Send(new GetOrdersByStatusQuery()));

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int count = 10)
            => Ok(await _mediator.Send(new GetRecentOrdersQuery(count)));
    }
}
