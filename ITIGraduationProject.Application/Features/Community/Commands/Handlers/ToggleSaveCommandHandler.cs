using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Commands.Handlers
{
    public class ToggleSaveCommandHandler
        : ResponseHandler,
          IRequestHandler<ToggleSaveCommand, Response<SaveStatusDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public ToggleSaveCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
            => (_uow, _currentUser) = (uow, currentUser);

        public async Task<Response<SaveStatusDto>> Handle(
            ToggleSaveCommand cmd, CancellationToken ct)
        {
            var template = await _uow.Templates.GetByIdAsync(cmd.TemplateId);
            if (template is null || template.IsDeleted)
                return NotFound<SaveStatusDto>("Template not found");

            var existingSave = await _uow.CommunityInteractions
                .GetTableAsTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    ci => ci.UserId == _currentUser.UserId
                       && ci.TemplateId == cmd.TemplateId
                       && ci.InteractionType == InteractionType.Save,
                    ct);

            if (existingSave is not null)
            {
                if (!existingSave.IsDeleted)
                {
                    existingSave.IsDeleted = true;
                    existingSave.DeletedAt = DateTime.UtcNow;
                }
                else
                {
                    existingSave.IsDeleted = false;
                    existingSave.DeletedAt = null;
                }
                _uow.CommunityInteractions.Update(existingSave);
                await _uow.SaveChangesAsync();

                return Success(new SaveStatusDto { Saved = !existingSave.IsDeleted });
            }

            var save = new CommunityInteraction
            {
                UserId = _currentUser.UserId,
                TemplateId = cmd.TemplateId,
                InteractionType = InteractionType.Save
            };

            await _uow.CommunityInteractions.AddAsync(save);
            await _uow.SaveChangesAsync();

            return Success(new SaveStatusDto { Saved = true });
        }
    }
}
