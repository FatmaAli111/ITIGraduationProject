using ITIGraduationProject.Application.Bases;
using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Community.Commands.Models
{
    public record DeleteCommentCommand(Guid Id) : IRequest<Response<string>>;
}
