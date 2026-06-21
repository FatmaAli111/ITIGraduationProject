using ITIGraduationProject.Application.Features.Rewards.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Rewards.Queries.Handlers
{
    public class GetUserRewardsQueryHandler : IRequestHandler<GetUserRewardsQuery, Response<List<Reward>>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public GetUserRewardsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<List<Reward>>> Handle(GetUserRewardsQuery request, CancellationToken cancellationToken)
        {
            var rewards = await _unitOfWork.Rewards.GetTableNoTracking()
                .Where(r => r.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            return new Response<List<Reward>>(rewards);
        }
        #endregion
    }
}