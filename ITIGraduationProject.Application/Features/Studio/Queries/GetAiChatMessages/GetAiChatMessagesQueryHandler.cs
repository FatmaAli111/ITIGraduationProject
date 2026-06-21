using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages
{
    public class GetAiChatMessagesQueryHandler : IRequestHandler<GetAiChatMessagesQuery, List<AiChatMessageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAiChatMessagesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AiChatMessageDto>> Handle(GetAiChatMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = _unitOfWork.AiChatSessions
                .GetTableNoTracking()
                .SelectMany(s => s.AiChatMessages)
                .Where(m => m.AiChatSessionId == request.SessionId)
                .OrderBy(m => m.SentAt)
                .ProjectToType<AiChatMessageDto>();

            return await messages.ToListAsync(cancellationToken);
        }
    }
}
