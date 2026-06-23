using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Notifications;
using ITIGraduationProject.Application.Features.Rewards.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Rewards.Commands.Handlers
{
    public class CalculateRewardsCommandHandler : IRequestHandler<CalculateRewardsCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CalculateRewardsCommandHandler> _logger;

        public CalculateRewardsCommandHandler(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<CalculateRewardsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(
            CalculateRewardsCommand request,
            CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users
                .GetTableAsTracking()
                .ToListAsync(cancellationToken);

            if (!users.Any())
                return new Response<string>("No users found to calculate rewards.");

            var userScores = users.Select(user =>
            {
                int interactionsCount = user.CommunityInteractions?.Count ?? 0;
                int ordersCount = user.Orders?.Count ?? 0;
                int calculatedPoints = (interactionsCount * 15) + (ordersCount * 30);

                return new { User = user, TotalPoints = calculatedPoints };
            })
            .OrderByDescending(u => u.TotalPoints)
            .ToList();

            for (int i = 0; i < userScores.Count; i++)
            {
                var item = userScores[i];
                item.User.TotalRewardPoints = item.TotalPoints;
                item.User.CurrentRank = i + 1;
                _unitOfWork.Users.Update(item.User);
            }

            var topUser = userScores.First().User;

            var userReward = new Reward
            {
                Id = Guid.NewGuid(),
                UserId = topUser.Id,
                RewardRuleId = Guid.NewGuid(),
                RewardType = RewardType.Badge,
                RewardValue = 100,
                IsClaimed = false,
                BadgeImageUrl = "top_1_badge.png"
            };

            await _unitOfWork.Rewards.AddAsync(userReward);
            await _unitOfWork.SaveChangesAsync();

            await NotificationDispatchHelper.TrySendAsync(
                _notificationService,
                _logger,
                topUser.Id,
                "Congratulations! You achieved 1st Place",
                $"Great job {topUser.Name}! You are officially the #1 designer on the platform based on your design interactions. A new achievement badge has been added to your profile!",
                NotificationType.RewardEarned);

            return new Response<string>(
                $"Success! Rewards calculated. Top 1 User is {topUser.UserName}, reward added successfully.");
        }
    }
}
