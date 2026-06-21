using MediatR;
using ITIGraduationProject.Application.Bases;

namespace ITIGraduationProject.Application.Features.Rewards.Commands.Models
{
    public class CalculateRewardsCommand : IRequest<Response<string>>
    {
    }
}