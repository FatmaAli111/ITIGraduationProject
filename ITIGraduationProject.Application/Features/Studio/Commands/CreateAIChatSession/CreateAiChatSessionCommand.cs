using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatSession
{
    public record CreateAiChatSessionCommand(Guid UserId, Guid ProductId) : IRequest<Guid>;
}
