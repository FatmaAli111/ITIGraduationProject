using ITIGraduationProject.Application.DTOS.Profiles;
using ITIGraduationProject.Application.Features.Profiles.Commands.Mapping;
using ITIGraduationProject.Application.Features.Profiles.Queries.Handlers;
using ITIGraduationProject.Application.Features.Profiles.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Profiles;

[TestFixture]
public class ProfilesQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IUserRepository> _usersRepo = null!;
    private Mock<IOrderRepository> _ordersRepo = null!;
    private Mock<ITemplateRepository> _templatesRepo = null!;
    private GetProfileHandler _handler = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        new ProfileMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _usersRepo = new Mock<IUserRepository>();
        _ordersRepo = new Mock<IOrderRepository>();
        _templatesRepo = new Mock<ITemplateRepository>();

        _uow.Setup(x => x.Users).Returns(_usersRepo.Object);
        _uow.Setup(x => x.Orders).Returns(_ordersRepo.Object);
        _uow.Setup(x => x.Templates).Returns(_templatesRepo.Object);

        _handler = new GetProfileHandler(_uow.Object);
    }

    [Test]
    public async Task GetProfile_Should_Return_Profile_Data()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Fatma",
            UserName = "fatma",
            Email = "fatma@test.com",
            Bio = "Designer",
            ProfileImageUrl = "/images/fatma.png",
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TotalAmount = 150m,
                OrderItems = new List<OrderItem>
                {
                    new() { Quantity = 2 },
                    new() { Quantity = 1 }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TotalAmount = 50m,
                OrderItems = new List<OrderItem>
                {
                    new() { Quantity = 1 }
                }
            }
        };

        var templates = new List<Template>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CreatorUserId = userId,
                LikesCount = 10,
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                CreatorUserId = userId,
                LikesCount = 20,
                IsDeleted = false
            }
        };

        _usersRepo.Setup(x => x.GetWithProfileCartAndPreferencesAsync(userId))
            .ReturnsAsync(user);

        _usersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<User> { user }.AsQueryable().BuildMock());

        _ordersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(orders.AsQueryable().BuildMock());

        _templatesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetProfileQuery(user.Email),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Name, Is.EqualTo("Fatma"));
        Assert.That(result.Data.UserName, Is.EqualTo("fatma"));
        Assert.That(result.Data.Email, Is.EqualTo("fatma@test.com"));
        Assert.That(result.Data.ProfilePictureUrl, Is.EqualTo("/images/fatma.png"));
        Assert.That(result.Data.DateJoined, Is.EqualTo(user.CreatedAt).Within(TimeSpan.FromSeconds(1)));
        Assert.That(result.Data.TotalOrdersCount, Is.EqualTo(2));
        Assert.That(result.Data.ItemsPurchasedCount, Is.EqualTo(4));
        Assert.That(result.Data.TotalSpent, Is.EqualTo(200m));
        Assert.That(result.Data.TemplatesCreatedCount, Is.EqualTo(2));
        Assert.That(result.Data.AvgTemplateRating, Is.EqualTo(15.0));
        Assert.That(result.Data.FollowersCount, Is.EqualTo(500));
        Assert.That(result.Data.FollowingCount, Is.EqualTo(488));
    }

    [Test]
    public async Task GetProfile_Should_Return_NotFound_When_User_NotFound()
    {
        const string email = "missing@test.com";

        _usersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<User>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetProfileQuery(email),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task GetProfile_Should_Return_Zero_Order_Stats_When_No_Orders()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Ali",
            UserName = "ali",
            Email = "ali@test.com",
            ProfileImageUrl = "/images/ali.png"
        };

        _usersRepo.Setup(x => x.GetWithProfileCartAndPreferencesAsync(userId))
            .ReturnsAsync(user);

        _usersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<User> { user }.AsQueryable().BuildMock());

        _ordersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Order>().AsQueryable().BuildMock());

        _templatesRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Template>().AsQueryable().BuildMock());

        var result = await _handler.Handle(
            new GetProfileQuery(user.Email),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.TotalOrdersCount, Is.EqualTo(0));
        Assert.That(result.Data.ItemsPurchasedCount, Is.EqualTo(0));
        Assert.That(result.Data.TotalSpent, Is.EqualTo(0m));
        Assert.That(result.Data.TemplatesCreatedCount, Is.EqualTo(0));
        Assert.That(result.Data.AvgTemplateRating, Is.EqualTo(0.0));
    }
}
