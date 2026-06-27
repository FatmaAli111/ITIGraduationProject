using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Bases.Templates
{
    public class GetTemplateByIdQueryHandler
    : ResponseHandler,
      IRequestHandler<GetTemplateByIdQuery, Response<TemplateDetailDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;

        public GetTemplateByIdQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _currentUserService = currentUserService;
        }

        public async Task<Response<TemplateDetailDto>> Handle(
            GetTemplateByIdQuery request, CancellationToken ct)
        {
            var template = await _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.Id == request.Id && !t.IsDeleted)
                .ProjectToType<TemplateDetailDto>()
                .FirstOrDefaultAsync(ct);

            if (template is null)
                return NotFound<TemplateDetailDto>("Template not found");

            Guid? currentUserId = null;
            try
            {
                currentUserId = _currentUserService.UserId;
            }
            catch
            {
                // User is anonymous
            }

            if (currentUserId.HasValue)
            {
                var userInteractions = await _uow.CommunityInteractions
                    .GetTableNoTracking()
                    .Where(ci => ci.UserId == currentUserId.Value
                              && ci.TemplateId == template.Id
                              && !ci.IsDeleted)
                    .ToListAsync(ct);

                template.LikedByCurrentUser = userInteractions.Any(ci => ci.InteractionType == InteractionType.Like);
                template.SavedByCurrentUser = userInteractions.Any(ci => ci.InteractionType == InteractionType.Save);
            }

            template.CommentCount = await _uow.CommunityInteractions
                .GetTableNoTracking()
                .CountAsync(ci => ci.TemplateId == template.Id
                               && ci.InteractionType == InteractionType.Comment
                               && !ci.IsDeleted, ct);

            return Success(template);
        }
    }
}
