using ITIGraduationProject.Application.Features.Rewards.Queries.Handlers;
using ITIGraduationProject.Application.Features.Rewards.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Rewards;

[TestFixture]
public class RewardsQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IRewardRepository> _rewardsRepo = null!;
    private GetUserRewardsQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _rewardsRepo = new Mock<IRewardRepository>();

        _uow.Setup(x => x.Rewards).Returns(_rewardsRepo.Object);

        _handler = new GetUserRewardsQueryHandler(_uow.Object);
    }

    [Test]
    public async Task GetUserRewards_Should_Return_Rewards_For_User()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var rewards = new List<Reward>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RewardType = RewardType.Badge,
                RewardValue = 100,
                IsClaimed = false,
                BadgeImageUrl = "badge.png"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RewardType = RewardType.Badge,
                RewardValue = 50,
                IsClaimed = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                RewardType = RewardType.Badge,
                RewardValue = 25
            }
        };

        _rewardsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(rewards.AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetUserRewardsQuery { UserId = userId },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Count, Is.EqualTo(2));
        Assert.That(result.Data.All(r => r.UserId == userId), Is.True);
    }

    [Test]
    public async Task GetUserRewards_Should_Return_Empty_List_When_No_Rewards()
    {
        var userId = Guid.NewGuid();

        _rewardsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Reward>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetUserRewardsQuery { UserId = userId },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!, Is.Empty);
    }
}
