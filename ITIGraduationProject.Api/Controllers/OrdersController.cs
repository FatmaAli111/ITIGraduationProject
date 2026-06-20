using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Features.Orders.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        #region Mediator Injection
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion

        #region Create Order Endpoint
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var response = await _mediator.Send(command);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        #endregion

        #region Display Orders By UserId Endpoint
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUsersOrders(Guid userId)
        {
            var response = await _mediator.Send(new GetUserOrdersQuery(userId));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response);

            return BadRequest(response);
        }
        #endregion
    }
}
