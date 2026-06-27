using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
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
        private readonly ICurrentUserService _currentUserService;

        public GetCommunityFeedQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _currentUserService = currentUserService;
        }

        public async Task<Response<PaginatedResult<FeedItemDto>>> Handle(
            GetCommunityFeedQuery request, CancellationToken ct)
        {
            var query = _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.IsPublic && !t.IsDeleted);

            var ordered = string.Equals(request.Filter, "new", System.StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.LikesCount);

            var result = await ordered
                .ProjectToType<FeedItemDto>()
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

            if (currentUserId.HasValue && result.Data.Any())
            {
                var templateIds = result.Data.Select(d => d.Id).ToList();
                var userInteractions = await _uow.CommunityInteractions
                    .GetTableNoTracking()
                    .Where(ci => ci.UserId == currentUserId.Value
                              && templateIds.Contains(ci.TemplateId)
                              && !ci.IsDeleted)
                    .ToListAsync(ct);

                var likedTemplateIds = userInteractions
                    .Where(ci => ci.InteractionType == InteractionType.Like)
                    .Select(ci => ci.TemplateId)
                    .ToHashSet();

                var savedTemplateIds = userInteractions
                    .Where(ci => ci.InteractionType == InteractionType.Save)
                    .Select(ci => ci.TemplateId)
                    .ToHashSet();

                foreach (var item in result.Data)
                {
                    item.LikedByCurrentUser = likedTemplateIds.Contains(item.Id);
                    item.SavedByCurrentUser = savedTemplateIds.Contains(item.Id);
                }
            }

            return Success(result);
        }
    }
}
