using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Queries.Handlers
{
    public class GetTemplateCommentsQueryHandler
        : ResponseHandler,
          IRequestHandler<GetTemplateCommentsQuery, Response<PaginatedResult<CommentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;

        public GetTemplateCommentsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _currentUserService = currentUserService;
        }

        public async Task<Response<PaginatedResult<CommentDto>>> Handle(
            GetTemplateCommentsQuery request, CancellationToken ct)
        {
            var result = await _uow.CommunityInteractions
                .GetTableNoTracking()
                .Where(ci => ci.TemplateId == request.TemplateId
                          && ci.InteractionType == InteractionType.Comment)
                .OrderByDescending(ci => ci.CreatedAt)
                .ProjectToType<CommentDto>()
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

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
                foreach (var comment in result.Data)
                {
                    comment.IsOwner = comment.UserId == currentUserId.Value;
                }
            }

            return Success(result);
        }
    }
}
