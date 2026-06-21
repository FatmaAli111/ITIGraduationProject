using ITIGraduationProject.Application.Features.Rewards.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Rewards.Commands.Handlers
{
    public class CalculateRewardsCommandHandler : IRequestHandler<CalculateRewardsCommand, Response<string>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public CalculateRewardsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<string>> Handle(CalculateRewardsCommand request, CancellationToken cancellationToken)
        {
            // 1. Fetch all users including their related collections for accurate calculation
            var users = await _unitOfWork.Users.GetTableAsTracking().ToListAsync(cancellationToken);

            if (!users.Any())
                return new Response<string>("No users found to calculate rewards.");

            // 2. Calculate scores based on active CommunityInteractions and Orders
            var userScores = users.Select(user =>
            {
                // Count community interactions (likes, saves, etc. on their designs) and total orders
                int interactionsCount = user.CommunityInteractions?.Count ?? 0;
                int ordersCount = user.Orders?.Count ?? 0;

                // Formula: 15 points per community interaction, 30 points per order placed
                int calculatedPoints = (interactionsCount * 15) + (ordersCount * 30);

                return new { User = user, TotalPoints = calculatedPoints };
            })
            .OrderByDescending(u => u.TotalPoints).ToList();

            // 3. Update users' total reward points and current ranks in the database
            for (int i = 0; i < userScores.Count; i++)
            {
                var item = userScores[i];
                item.User.TotalRewardPoints = item.TotalPoints;
                item.User.CurrentRank = i + 1;

                _unitOfWork.Users.Update(item.User);
            }

            // 4. Reward the Top 1 Rated User
            var topUser = userScores.First().User;

            // Create a new Reward using the AIAndModeration Reward entity
            var userReward = new ITIGraduationProject.Domain.Entities.AIAndModeration.Reward
            {
                Id = Guid.NewGuid(),
                UserId = topUser.Id,
                RewardRuleId = Guid.NewGuid(), // Link with an actual active rule id if available
                RewardType = RewardType.Badge,
                RewardValue = 100,
                IsClaimed = false,
                BadgeImageUrl = "top_1_badge.png"
            };

            await _unitOfWork.Rewards.AddAsync(userReward);

            // 5. Send a congratulations notification to the Top 1 User
            var congratulateNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = topUser.Id,
                Type = NotificationType.RewardEarned,
                Title = "🎉 Congratulations! You achieved 1st Place",
                Message = $"Great job {topUser.Name}! You are officially the #1 designer on the platform based on your design interactions. A new achievement badge has been added to your profile!",
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(congratulateNotification);

            // 6. Save all tracking changes safely using a single atomic transaction
            await _unitOfWork.SaveChangesAsync();

            return new Response<string>($"Success! Rewards calculated. Top 1 User is {topUser.UserName}, Reward added and Notification sent successfully.");
        }
        #endregion

    }
}