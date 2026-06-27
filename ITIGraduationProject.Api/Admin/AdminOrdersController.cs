using ITIGraduationProject.Application.Features.Admin.Orders.Queries.Models;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminOrdersController(IMediator mediator) => _mediator = mediator;

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] GetAllOrdersQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPatch("orders/{id:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusCommand command)
        {
            command.OrderId = id;
            var updated = await _mediator.Send(command);

            if (!updated)
            {
                return NotFound(new Response<string>
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Succeeded = false,
                    Message = "Order not found or update failed."
                });
            }

            return Ok(new Response<string>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Order status updated successfully."
            });
        }

        [HttpPatch("order-items/{id}/assign-printer")]
        public async Task<IActionResult> AssignPrinterToOrderItem(Guid id, AssignPrinterToOrderItemCommand command)
        {
            command.OrderItemId = id;
            return Ok(await _mediator.Send(command));
        }
    }
}
