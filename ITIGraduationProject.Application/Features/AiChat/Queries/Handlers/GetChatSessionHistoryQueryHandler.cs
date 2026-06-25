using ITIGraduationProject.Application.Features.AiChat.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.AiChat;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.AiChat.Queries.Handlers
{
    public class GetChatSessionHistoryQueryHandler : IRequestHandler<GetChatSessionHistoryQuery, Response<ChatHistoryDto>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public GetChatSessionHistoryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<ChatHistoryDto>> Handle(GetChatSessionHistoryQuery request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.AiChatSessions.GetWithMessagesAsync(request.SessionId);

            if (session == null)
                return new Response<ChatHistoryDto>("Chat session not found.");

            var chatHistoryDto = new ChatHistoryDto
            {
                SessionId = session.Id,
                SessionType = session.SessionType.ToString(),
                CreatedAt = System.DateTime.UtcNow,
                Messages = session.AiChatMessages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    MessageText = m.MessageText,
                    SentAt = m.SentAt
                }).OrderBy(m => m.SentAt).ToList()
            };

            return new Response<ChatHistoryDto>(chatHistoryDto);
        }
        #endregion
    }
}