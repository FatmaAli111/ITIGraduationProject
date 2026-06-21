using MediatR;
using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages
{
    public record GetAiChatMessagesQuery(Guid SessionId) : IRequest<List<AiChatMessageDto>>;
}
