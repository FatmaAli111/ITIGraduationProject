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
    public class GetCommunityFeedQueryHandler
        : ResponseHandler,
          IRequestHandler<GetCommunityFeedQuery, Response<PaginatedResult<FeedItemDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetCommunityFeedQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<PaginatedResult<FeedItemDto>>> Handle(
            GetCommunityFeedQuery request, CancellationToken ct)
        {
            var query = _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.IsPublic && !t.IsDeleted)
                .Select(t => new FeedItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    PreviewImageURL = t.PreviewImageURL,
                    StyleTags = t.StyleTags,
                    LikesCount = t.LikesCount,
                    RemixesCount = t.RemixesCount,
                    CommentCount = t.CommunityInteractions.Count(
                        ci => ci.InteractionType == InteractionType.Comment),
                    AverageRating = t.AverageRating,
                    CreatorUserId = t.CreatorUserId,
                    CreatorName = t.CreatorUser.Name,
                    CreatorProfileImageUrl = t.CreatorUser.ProfileImageUrl,
                    CreatedAt = t.CreatedAt
                });

            query = string.Equals(request.Filter, "new", System.StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.LikesCount);

            var result = await query.ToPaginatedListAsync(request.PageNumber, request.PageSize);
            return Success(result);
        }
    }
}
