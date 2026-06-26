using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Service.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Admin;

[TestFixture]
public class AdminUserServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManager;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IEmailService> _emailService;
    private Mock<IConfiguration> _configuration;

    private AdminUserService _service;

    [SetUp]
    public void Setup()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        _userManager =
            new Mock<UserManager<ApplicationUser>>
            (
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

        _unitOfWork = new Mock<IUnitOfWork>();
        _emailService = new Mock<IEmailService>();
        _configuration = new Mock<IConfiguration>();

        _service = new AdminUserService(
            _userManager.Object,
            _unitOfWork.Object,
            _emailService.Object,
            _configuration.Object);
    }

    [Test]
    public async Task InviteUserAsync_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        var request = new InviteUserRequestDTO
        {
            Email = "admin@test.com",
            Name = "Fatma",
            Role = Roles.Admin
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());

        var result = await _service.InviteUserAsync(request);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Email already exists."));
    }

    [Test]
    public async Task InviteUserAsync_Should_Return_BadRequest_When_Role_Invalid()
    {
        var request = new InviteUserRequestDTO
        {
            Email = "admin@test.com",
            Name = "Fatma",
            Role = "Manager"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _service.InviteUserAsync(request);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message,
            Is.EqualTo("Invalid role for invitation."));
    }

    [Test]
    public async Task GetUserByIdAsync_Should_Return_NotFound_When_User_Not_Exists()
    {
        _unitOfWork
            .Setup(x => x.Users.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Identity.User)null);

        var result =
            await _service.GetUserByIdAsync(Guid.NewGuid());

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message,
            Is.EqualTo("User not found."));
    }

    [Test]
    public async Task UpdateUserAsync_Should_Return_NotFound_When_User_Not_Exists()
    {
        _unitOfWork
            .Setup(x => x.Users.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Identity.User)null);

        var result =
            await _service.UpdateUserAsync(
                Guid.NewGuid(),
                new UpdateUserRequestDTO
                {
                    Name = "Updated"
                });

        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public async Task ChangeUserStatusAsync_Should_Return_NotFound_When_User_Not_Exists()
    {
        _unitOfWork
            .Setup(x => x.Users.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Identity.User)null);

        var result =
            await _service.ChangeUserStatusAsync(
                Guid.NewGuid(),
                true);

        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public async Task ChangeUserRoleAsync_Should_Return_BadRequest_When_Role_Invalid()
    {
        var result =
            await _service.ChangeUserRoleAsync(
                Guid.NewGuid(),
                "WrongRole");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message,
            Is.EqualTo("Invalid role."));
    }

    [Test]
    public async Task ChangeUserRoleAsync_Should_Return_NotFound_When_User_Not_Exists()
    {
        _userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var result =
            await _service.ChangeUserRoleAsync(
                Guid.NewGuid(),
                Roles.Admin);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message,
            Is.EqualTo("User not found."));
    }

    [Test]
    public async Task GetUserRoleAsync_Should_Return_Empty_When_User_Not_Found()
    {
        _userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var result =
            await _service.GetUserRoleAsync(Guid.NewGuid());

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetUserRoleAsync_Should_Return_User_Role()
    {
        var user = new ApplicationUser();

        _userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                Roles.Admin
            });

        var result =
            await _service.GetUserRoleAsync(Guid.NewGuid());

        Assert.That(result, Is.EqualTo(Roles.Admin));
    }
}