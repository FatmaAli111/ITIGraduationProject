using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ReportGenerator;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Service.ReportGenerator
{
    public class ReportChatService : ResponseHandler, IReportChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILangflowService _langflowService;

    public ReportChatService(
        IUnitOfWork unitOfWork,
        ILangflowService langflowService)
        {
            _unitOfWork = unitOfWork;
            _langflowService = langflowService;
        }

        public async Task<Response<Guid>> CreateSessionAsync(Guid userId)
        {
            var session = new AiChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionType = AiChatSessionType.ReportGeneration
            };

            await _unitOfWork.AiChatSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return Created(session.Id);
        }

        public async Task<Response<ReportChatResponseDto>> SendMessageAsync(
            Guid sessionId,
            string message)
        {
            var session =
     await _unitOfWork.AiChatSessions
         .GetWithMessagesTrackingAsync(sessionId);

            if (session == null)
                return NotFound<ReportChatResponseDto>("Session not found");

            if (session == null)
                return NotFound<ReportChatResponseDto>(
                    "Session not found");

            var userMessage = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = sessionId,
                Sender = "user",
                MessageText = message,
                SentAt = DateTime.UtcNow
            };

            session.AiChatMessages.Add(userMessage);

            var aiText = "test response";
            var aiMessage = new AiChatMessage
            {
                Id = Guid.NewGuid(),
                AiChatSessionId = sessionId,
                Sender = "ai",
                MessageText = aiText,
                SentAt = DateTime.UtcNow
            };

            session.AiChatMessages.Add(aiMessage);

            _unitOfWork.AiChatSessions.Update(session);

            await _unitOfWork.SaveChangesAsync();

            return Success(new ReportChatResponseDto
            {
                SessionId = sessionId,
                UserMessageId = userMessage.Id,
                AiMessageId = aiMessage.Id,
                Response = aiText,
                ResponseTime = aiMessage.SentAt
            });
        }

        public async Task<Response<List<ReportChatMessageDto>>> GetHistoryAsync(
            Guid sessionId)
        {
            var session =
                await _unitOfWork.AiChatSessions
                    .GetWithMessagesAsync(sessionId);

            if (session == null)
                return NotFound<List<ReportChatMessageDto>>(
                    "Session not found");

            var result = session.AiChatMessages
                .OrderBy(x => x.SentAt)
                .Select(x => new ReportChatMessageDto
                {
                    Id = x.Id,
                    Sender = x.Sender,
                    Message = x.MessageText,
                    SentAt = x.SentAt
                })
                .ToList();

            return Success(result);
        }
    }

}
