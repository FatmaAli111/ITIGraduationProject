using MediatR;
using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions
{
    public record GetUserAiChatSessionsQuery(Guid UserId) : IRequest<List<AiChatSessionDto>>;
}
