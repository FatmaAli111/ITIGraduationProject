using ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions
{
    public class GetUserAiChatSessionsQueryHandler : IRequestHandler<GetUserAiChatSessionsQuery, List<AiChatSessionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserAiChatSessionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AiChatSessionDto>> Handle(GetUserAiChatSessionsQuery request, CancellationToken cancellationToken)
        {
            var sessions = _unitOfWork.AiChatSessions
                .GetTableNoTracking()
                .Where(s => s.UserId == request.UserId)
                .OrderByDescending(s => s.CreatedAt)
                .ProjectToType<AiChatSessionDto>();

            return await sessions.ToListAsync(cancellationToken);
        }
    }
}
