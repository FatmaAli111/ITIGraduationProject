using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public GetTemplateCommentsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<PaginatedResult<CommentDto>>> Handle(
            GetTemplateCommentsQuery request, CancellationToken ct)
        {
            var query = _uow.CommunityInteractions
                .GetTableNoTracking()
                .Where(ci => ci.TemplateId == request.TemplateId
                          && ci.InteractionType == InteractionType.Comment)
                .OrderByDescending(ci => ci.CreatedAt)
                .Select(ci => new CommentDto
                {
                    Id = ci.Id,
                    Content = ci.Content ?? string.Empty,
                    UserId = ci.UserId,
                    UserName = ci.User.Name,
                    UserProfileImageUrl = ci.User.ProfileImageUrl,
                    CreatedAt = ci.CreatedAt
                });

            var result = await query.ToPaginatedListAsync(request.PageNumber, request.PageSize);
            return Success(result);
        }
    }
}
