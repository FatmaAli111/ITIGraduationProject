using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Community.Queries.Handlers
{
    public class GetTopCreatorsQueryHandler
        : ResponseHandler,
          IRequestHandler<GetTopCreatorsQuery, Response<List<TopCreatorDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetTopCreatorsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<List<TopCreatorDto>>> Handle(
            GetTopCreatorsQuery request, CancellationToken ct)
        {
            var creators = await _uow.Templates
                .GetTableNoTracking()
                .Where(t => t.IsPublic && !t.IsDeleted)
                .GroupBy(t => t.CreatorUserId)
                .Select(g => new TopCreatorDto
                {
                    UserId = g.Key,
                    UserName = g.First().CreatorUser.Name,
                    ProfileImageUrl = g.First().CreatorUser.ProfileImageUrl,
                    TotalLikes = g.Sum(t => t.LikesCount),
                    TotalRemixes = g.Sum(t => t.RemixesCount),
                    TemplateCount = g.Count()
                })
                .OrderByDescending(c => c.TotalLikes + c.TotalRemixes * 2)
                .Take(request.Count)
                .ToListAsync(ct);

            return Success(creators);
        }
    }
}
