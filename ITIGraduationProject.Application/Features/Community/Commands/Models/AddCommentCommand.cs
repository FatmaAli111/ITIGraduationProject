using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record AddCommentCommand(Guid TemplateId, string Content) : IRequest<Response<CommentDto>>;
}
