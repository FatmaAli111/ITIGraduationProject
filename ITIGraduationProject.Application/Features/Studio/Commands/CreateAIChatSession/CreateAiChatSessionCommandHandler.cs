using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using Mapster;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatSession
{
    public class CreateAiChatSessionCommandHandler : IRequestHandler<CreateAiChatSessionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateAiChatSessionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateAiChatSessionCommand request, CancellationToken cancellationToken)
        {
            var session = new AiChatSession
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CurrentDesignId = request.ProductId,
                SessionType = Domain.Enums.AiChatSessionType.DesignAssistance
            };

            await _unitOfWork.AiChatSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return session.Id;
        }
    }
}
