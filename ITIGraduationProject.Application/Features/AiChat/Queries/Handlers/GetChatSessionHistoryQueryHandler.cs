using ITIGraduationProject.Application.Features.AiChat.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.AiChat.Queries.Handlers
{
    public class GetChatSessionHistoryQueryHandler : IRequestHandler<GetChatSessionHistoryQuery, Response<AiChatSession>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public GetChatSessionHistoryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<AiChatSession>> Handle(GetChatSessionHistoryQuery request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.AiChatSessions.GetWithMessagesAsync(request.SessionId);

            if (session == null)
                return new Response<AiChatSession>("Chat session not found.");

            return new Response<AiChatSession>(session);
        }
        #endregion
    }
}