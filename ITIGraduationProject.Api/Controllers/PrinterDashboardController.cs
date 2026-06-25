using ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Models;
using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/printer")]
    [ApiController]
    [Authorize(Roles = Roles.Printer)]
    public class PrinterDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PrinterDashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet("order-items")]
        public async Task<IActionResult> GetMyAssignedOrderItems([FromQuery] GetMyAssignedOrderItemsQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPatch("order-items/{id}/status")]
        public async Task<IActionResult> UpdateOrderItemStatus(Guid id, UpdateOrderItemStatusCommand command)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("profile/summary")]
        public async Task<IActionResult> GetMyPrinterProfileSummary()
            => Ok(await _mediator.Send(new GetMyPrinterProfileSummaryQuery()));
    }
}
