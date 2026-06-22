using MediatR;
using System;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatMessage
{
    public record CreateAiChatMessageCommand(Guid SessionId, string UserMessage) : IRequest<Guid>;
}
