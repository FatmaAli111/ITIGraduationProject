using ITIGraduationProject.Application.Features.Templates.Commands.Models;
using ITIGraduationProject.Application.Features.Templates.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TemplatesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetPublicTemplates([FromQuery] GetPublicTemplatesQuery query)
            => Ok(await _mediator.Send(query));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await _mediator.Send(new GetTemplateByIdQuery(id)));

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyTemplates([FromQuery] GetMyTemplatesQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTemplateCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateCommand cmd)
            => Ok(await _mediator.Send(cmd with { Id = id }));

        [HttpPost("{id:guid}/publish")]
        [Authorize]
        public async Task<IActionResult> Publish(Guid id)
            => Ok(await _mediator.Send(new PublishTemplateCommand(id)));

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
            => Ok(await _mediator.Send(new DeleteTemplateCommand(id)));
    }
}