using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Wrapers;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Queries.Models
{
    public record GetTemplateCommentsQuery(
        Guid TemplateId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Response<PaginatedResult<CommentDto>>>;
}
