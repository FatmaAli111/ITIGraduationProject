using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Wrapers;
using MediatR;

namespace ITIGraduationProject.Application.Features.Community.Queries.Models
{
    public record GetCommunityFeedQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Filter = null
    ) : IRequest<Response<PaginatedResult<FeedItemDto>>>;
}
