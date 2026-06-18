using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers.Templates.CQRS;
using ITIGraduationProject.Application.Wrapers.Templates.DTOs;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class CreateTemplateCommandHandler
    : ResponseHandler,
      IRequestHandler<CreateTemplateCommand, Response<TemplateDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateTemplateCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<TemplateDto>> Handle(
            CreateTemplateCommand cmd, CancellationToken ct)
        {
            var template = new Template
            {
                CategoryId = cmd.CategoryId,
                CreatorUserId = _currentUser.UserId,
                Name = cmd.Name,
                StyleTags = cmd.StyleTags,
                PreviewImageURL = cmd.PreviewImageURL,
                IsPublic = cmd.IsPublic,
            };

            await _uow.Templates.AddAsync(template);
            await _uow.SaveChangesAsync();

            return Created(template.Adapt<TemplateDto>());
        }
    }
}
