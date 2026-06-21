using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record ToggleLikeCommand(Guid TemplateId) : IRequest<Response<LikeStatusDto>>;
}
