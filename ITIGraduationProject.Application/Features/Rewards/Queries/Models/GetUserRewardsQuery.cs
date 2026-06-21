using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using MediatR;
using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.Features.Rewards.Queries.Models
{
    public class GetUserRewardsQuery : IRequest<Response<List<Reward>>>
    {
        public Guid UserId { get; set; }
    }
}