using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using MediatR;

namespace ITIGraduationProject.Application.Features.Community.Queries.Models
{
    public record GetModerationReportsQuery(
        int PageNumber = 1,
        int PageSize = 10,
        ModerationReportStatus? Status = null
    ) : IRequest<Response<PaginatedResult<ModerationReportDto>>>;
}
