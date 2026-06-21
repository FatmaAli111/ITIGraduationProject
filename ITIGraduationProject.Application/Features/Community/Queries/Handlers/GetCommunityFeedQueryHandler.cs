using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Wrapers;
using Mapster;
using MediatR;
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
                .Where(t => t.IsPublic && !t.IsDeleted);

            var ordered = string.Equals(request.Filter, "new", System.StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderByDescending(t => t.LikesCount);

            var result = await ordered
                .ProjectToType<FeedItemDto>()
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Success(result);
        }
    }
}
