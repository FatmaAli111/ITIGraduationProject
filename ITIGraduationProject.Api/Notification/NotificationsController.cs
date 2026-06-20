using ITIGraduationProject.Application.Features.Notification.Commands.Models;
using ITIGraduationProject.Application.Features.Notification.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Notification
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;


        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _mediator.Send(
                new GetNotificationsQuery());

            return Ok(response);
        }


        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var response = await _mediator.Send(
                new GetUnreadNotificationsQuery());

            return Ok(response);
        }


        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var response = await _mediator.Send(
                new MarkNotificationAsReadCommand(id));

            return Ok(response);
        }


        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var response = await _mediator.Send(
                new MarkAllNotificationsAsReadCommand());

            return Ok(response);
        }
}
}
