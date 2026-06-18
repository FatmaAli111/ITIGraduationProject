using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Templates.CQRS;
using ITIGraduationProject.Application.Wrapers.Templates.DTOs;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class UpdateTemplateCommandHandler
    : ResponseHandler,
      IRequestHandler<UpdateTemplateCommand, Response<TemplateDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<TemplateDto>> Handle(
            UpdateTemplateCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.Id);

            if (template is null || template.IsDeleted)
                return NotFound<TemplateDto>("Template not found");

            if (template.CreatorUserId != _currentUser.UserId)
                return Unauthorized<TemplateDto>();

            if (cmd.Name is not null) template.Name = cmd.Name;
            if (cmd.StyleTags is not null) template.StyleTags = cmd.StyleTags;
            if (cmd.PreviewImageURL is not null) template.PreviewImageURL = cmd.PreviewImageURL;
            template.UpdatedAt = DateTime.UtcNow;

            _uow.Templates.Update(template);
            await _uow.SaveChangesAsync();

            return Success(template.Adapt<TemplateDto>());
        }
    }
}
