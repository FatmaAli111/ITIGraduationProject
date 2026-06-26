using ITIGraduationProject.Application.Features.AiChat.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces; // Required to resolve IAiService
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Interfaces.IAiService;

namespace ITIGraduationProject.Application.Features.AiChat.Commands.Handlers
{
    public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Response<string>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;

        public SendChatMessageCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }
        #endregion

        #region Handle Method
        public async Task<Response<string>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
        {
            // Check if the chat session exists, if not create a new one
            var session = await _unitOfWork.AiChatSessions.GetWithMessagesAsync(request.SessionId);
            if (session == null)
            {
                session = new AiChatSession
                {
                    Id = request.SessionId,
                    UserId = request.UserId,
                    SessionType = AiChatSessionType.General
                };
                await _unitOfWork.AiChatSessions.AddAsync(session);
            }

            var userMsg = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = session.Id,
                Sender = "User",
                MessageText = request.MessageText,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.AiChatMessages.AddAsync(userMsg);


            string aiReplyText = await _aiService.AskGeminiAsync(request.MessageText);


            var aiMsg = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = session.Id,
                Sender = "AI_Assistant",
                MessageText = aiReplyText,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.AiChatMessages.AddAsync(aiMsg);


            await _unitOfWork.SaveChangesAsync();
            return new Response<string>(aiReplyText);
        }
        #endregion
    }
}