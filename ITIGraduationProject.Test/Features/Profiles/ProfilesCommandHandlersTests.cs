using ITIGraduationProject.Application.Features.Profiles.Commands.Handlers;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Profiles;

[TestFixture]
public class ProfilesCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IUserRepository> _usersRepo = null!;
    private UpdateProfileHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _usersRepo = new Mock<IUserRepository>();

        _uow.Setup(x => x.Users).Returns(_usersRepo.Object);

        _handler = new UpdateProfileHandler(_uow.Object);
    }

    [Test]
    public async Task UpdateProfile_Should_Succeed_When_No_Image()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Old Name",
            UserName = "olduser",
            Email = "old@test.com",
            Bio = "Old bio"
        };

        _usersRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateProfileCommand
        {
            UserId = userId.ToString(),
            Name = "New Name",
            UserName = "newuser",
            Email = "new@test.com",
            Bio = "Updated bio"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.EqualTo("Success"));
        Assert.That(user.Name, Is.EqualTo("New Name"));
        Assert.That(user.UserName, Is.EqualTo("newuser"));
        Assert.That(user.Email, Is.EqualTo("new@test.com"));
        Assert.That(user.Bio, Is.EqualTo("Updated bio"));

        _usersRepo.Verify(x => x.Update(user), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateProfile_Should_Return_NotFound_When_User_NotFound()
    {
        var userId = Guid.NewGuid();

        _usersRepo.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);
        _usersRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<User>().AsQueryable().BuildMock());

        var command = new UpdateProfileCommand
        {
            UserId = userId.ToString(),
            Name = "Name",
            UserName = "user",
            Email = "test@test.com",
            Bio = "bio"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("User not found."));

        _usersRepo.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task UpdateProfile_Should_Return_BadRequest_When_Save_Fails()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Name",
            UserName = "user",
            Email = "test@test.com"
        };

        _usersRepo.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        var command = new UpdateProfileCommand
        {
            UserId = userId.ToString(),
            Name = "Updated",
            UserName = "updated",
            Email = "updated@test.com",
            Bio = "bio"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("No changes were saved to the database."));

        _usersRepo.Verify(x => x.Update(user), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
