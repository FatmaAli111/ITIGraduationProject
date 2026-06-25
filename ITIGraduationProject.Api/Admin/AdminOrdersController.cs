using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Admin
{
    [ApiController]
    [Route("api/admin/order-items")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminOrdersController(IMediator mediator) => _mediator = mediator;

        [HttpPatch("{id}/assign-printer")]
        public async Task<IActionResult> AssignPrinterToOrderItem(Guid id, AssignPrinterToOrderItemCommand command)
        {
            command.OrderItemId = id;
            return Ok(await _mediator.Send(command));
        }
    }
}
