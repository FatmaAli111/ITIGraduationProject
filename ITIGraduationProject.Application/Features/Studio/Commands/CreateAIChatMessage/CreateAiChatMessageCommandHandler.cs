using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatMessage
{
    public class CreateAiChatMessageCommandHandler : IRequestHandler<CreateAiChatMessageCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateAiChatMessageCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateAiChatMessageCommand request, CancellationToken cancellationToken)
        {
            var session = await _unitOfWork.AiChatSessions
                .GetTableAsTracking()
                .Include(s => s.AiChatMessages)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
                throw new KeyNotFoundException("AI chat session not found.");
            
            var userMessage = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = request.SessionId,
                Sender = "user",
                MessageText = request.UserMessage,
                SentAt = DateTime.UtcNow
            };

            session.AiChatMessages.Add(userMessage);

            var aiResponse = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = request.SessionId,
                Sender = "ai",
                MessageText = "This is a simulated AI response to: " + request.UserMessage,
                SentAt = DateTime.UtcNow
            };

            session.AiChatMessages.Add(aiResponse);

            _unitOfWork.AiChatSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return userMessage.Id;
        }
    }
}
