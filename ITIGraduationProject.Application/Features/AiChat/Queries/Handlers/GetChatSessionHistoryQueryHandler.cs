using ITIGraduationProject.Application.Features.AiChat.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.DTOS.AiChat;

namespace ITIGraduationProject.Application.Features.AiChat.Queries.Handlers
{
    public class GetChatSessionHistoryQueryHandler :ResponseHandler, IRequestHandler<GetChatSessionHistoryQuery, Response<ChatHistoryDto>>
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
                return  BadRequest<ChatHistoryDto>("Chat session not found.");
         
            var dto = new ChatHistoryDto
            {
                SessionId = session.Id,
                Messages = session.AiChatMessages
                    .OrderBy(x => x.SentAt)
                    .Select(x => new ChatMessageDto
                    {
                        Id = x.Id,
                        Sender = x.Sender,
                        MessageText = x.MessageText,
                        SentAt = x.SentAt
                    })
                    .ToList()
            };

            return Success<ChatHistoryDto>(dto);
        }
        #endregion
    }
}