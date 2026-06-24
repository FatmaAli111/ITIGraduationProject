using ITIGraduationProject.Application.DTOS.PrinterProfiles;
using ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Models;
using ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrinterProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PrinterProfilesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [Authorize(Roles = Roles.Printer)]
        public async Task<IActionResult> Create([FromBody] CreatePrinterProfileCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAll([FromQuery] GetPrinterProfilesQuery query)
            => Ok(await _mediator.Send(query));
    }
}
