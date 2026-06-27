using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Rewards.Commands.Handlers;
using ITIGraduationProject.Application.Features.Rewards.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Rewards;

[TestFixture]
public class RewardsCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IUserRepository> _usersRepo = null!;
    private Mock<IRewardRepository> _rewardsRepo = null!;
    private Mock<INotificationService> _notificationService = null!;
    private Mock<ILogger<CalculateRewardsCommandHandler>> _logger = null!;
    private CalculateRewardsCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _usersRepo = new Mock<IUserRepository>();
        _rewardsRepo = new Mock<IRewardRepository>();
        _notificationService = new Mock<INotificationService>();
        _logger = new Mock<ILogger<CalculateRewardsCommandHandler>>();

        _uow.Setup(x => x.Users).Returns(_usersRepo.Object);
        _uow.Setup(x => x.Rewards).Returns(_rewardsRepo.Object);

        _handler = new CalculateRewardsCommandHandler(
            _uow.Object,
            _notificationService.Object,
            _logger.Object);
    }

    [Test]
    public async Task CalculateRewards_Should_Succeed_When_Users_Exist()
    {
        var topUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = topUserId,
                Name = "Fatma",
                UserName = "fatma",
                CommunityInteractions = new List<CommunityInteraction>
                {
                    new(), new(), new()
                },
                Orders = new List<Order> { new() }
            },
            new()
            {
                Id = otherUserId,
                Name = "Ali",
                UserName = "ali",
                CommunityInteractions = new List<CommunityInteraction> { new() },
                Orders = new List<Order>()
            }
        };

        _usersRepo.Setup(x => x.GetTableAsTracking())
            .Returns(users.AsQueryable().BuildMock());

        _rewardsRepo.Setup(x => x.AddAsync(It.IsAny<Reward>()))
            .ReturnsAsync((Reward r) => r);

        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _notificationService.Setup(x => x.SendNotificationAsync(
                topUserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.RewardEarned))
            .ReturnsAsync(new Response<bool>(true));

        var result = await _handler.Handle(new CalculateRewardsCommand(), CancellationToken.None);

        Assert.That(result.Message, Does.Contain("fatma"));
        Assert.That(result.Message, Does.Contain("Success! Rewards calculated"));

        var topUser = users.Single(u => u.Id == topUserId);
        var otherUser = users.Single(u => u.Id == otherUserId);

        Assert.That(topUser.TotalRewardPoints, Is.EqualTo(75));
        Assert.That(topUser.CurrentRank, Is.EqualTo(1));
        Assert.That(otherUser.TotalRewardPoints, Is.EqualTo(15));
        Assert.That(otherUser.CurrentRank, Is.EqualTo(2));

        _usersRepo.Verify(x => x.Update(It.IsAny<User>()), Times.Exactly(2));
        _rewardsRepo.Verify(x => x.AddAsync(It.Is<Reward>(r =>
            r.UserId == topUserId &&
            r.RewardType == RewardType.Badge &&
            r.RewardValue == 100 &&
            !r.IsClaimed)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);

        _notificationService.Verify(x => x.SendNotificationAsync(
            topUserId,
            "Congratulations! You achieved 1st Place",
            It.IsAny<string>(),
            NotificationType.RewardEarned), Times.Once);
    }

    [Test]
    public async Task CalculateRewards_Should_Return_Message_When_No_Users_Found()
    {
        _usersRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<User>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new CalculateRewardsCommand(), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("No users found to calculate rewards."));

        _usersRepo.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _rewardsRepo.Verify(x => x.AddAsync(It.IsAny<Reward>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
        _notificationService.Verify(x => x.SendNotificationAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>()), Times.Never);
    }
}
