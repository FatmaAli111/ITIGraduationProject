using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using MediatR;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Community.Queries.Models
{
    public record GetTopCreatorsQuery(int Count = 10) : IRequest<Response<List<TopCreatorDto>>>;
}
