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
    public class GetModerationReportsQueryHandler
        : ResponseHandler,
          IRequestHandler<GetModerationReportsQuery, Response<PaginatedResult<ModerationReportDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetModerationReportsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Response<PaginatedResult<ModerationReportDto>>> Handle(
            GetModerationReportsQuery request, CancellationToken ct)
        {
            var query = _uow.ModerationReports
                .GetTableNoTracking()
                .Where(r => !r.IsDeleted);

            if (request.Status.HasValue)
                query = query.Where(r => r.Status == request.Status.Value);

            var result = await query
                .OrderByDescending(r => r.CreatedAt)
                .ProjectToType<ModerationReportDto>()
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Success(result);
        }
    }
}
