using ITIGraduationProject.Application.DTOS.Admin;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Service.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private Mock<ILogger<AdminUserService>> _logger;

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
        _logger = new Mock<ILogger<AdminUserService>>();

        _service = new AdminUserService(
            _userManager.Object,
            _unitOfWork.Object,
            _emailService.Object,
            _configuration.Object,
            _logger.Object);
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
    public async Task InviteUserAsync_Should_Reject_Invalid_Printer_Flags()
    {
        var request = new InviteUserRequestDTO
        {
            Email = "printer@test.com",
            Name = "Printer",
            Role = Roles.Printer,
            SupportedFabrics = (FabricType)32,
            SupportedPrintMethods = PrintMethodType.ScreenPrinting
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _service.InviteUserAsync(request);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Does.Contain("valid fabric and print method"));
    }

    [Test]
    public async Task InviteUserAsync_Should_Roll_Back_When_Email_Sending_Fails()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        _unitOfWork.Setup(x => x.Users).Returns(userRepository.Object);
        _unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _configuration
            .Setup(x => x["ClientSettings:ClientBaseUrl"])
            .Returns("http://localhost:4200");

        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Admin))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("reset-token");
        _userManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _emailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var result = await _service.InviteUserAsync(new InviteUserRequestDTO
        {
            Email = "admin-invite@test.com",
            Name = "Invited Admin",
            Role = Roles.Admin
        });

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invitation email could not be sent. No user was created."));
        _userManager.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
        userRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
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
    public async Task ResendInvitationAsync_Should_Send_New_Acceptance_Link()
    {
        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "pending-invite@test.com",
            EmailConfirmed = false
        };

        _configuration
            .Setup(x => x["ClientSettings:ClientBaseUrl"])
            .Returns("http://localhost:4200");
        _userManager
            .Setup(x => x.FindByIdAsync(applicationUser.Id.ToString()))
            .ReturnsAsync(applicationUser);
        _userManager
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { Roles.Printer });
        _userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(applicationUser))
            .ReturnsAsync("invitation-token");

        var result = await _service.ResendInvitationAsync(applicationUser.Id);

        Assert.That(result.Succeeded, Is.True);
        _emailService.Verify(x => x.SendEmailAsync(
            applicationUser.Email,
            "Invitation - Printer",
            It.Is<string>(body => body.Contains("/accept-invitation?userId="))), Times.Once);
    }

    [Test]
    public async Task ChangeUserRoleAsync_Should_Replace_Identity_Role()
    {
        var applicationUser = new ApplicationUser { Id = Guid.NewGuid() };

        _userManager
            .Setup(x => x.FindByIdAsync(applicationUser.Id.ToString()))
            .ReturnsAsync(applicationUser);
        _userManager
            .Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { Roles.User });
        _userManager
            .Setup(x => x.RemoveFromRolesAsync(applicationUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.AddToRoleAsync(applicationUser, Roles.Printer))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.ChangeUserRoleAsync(applicationUser.Id, Roles.Printer);

        Assert.That(result.Succeeded, Is.True);
        _userManager.Verify(
            x => x.RemoveFromRolesAsync(applicationUser, It.Is<IEnumerable<string>>(roles => roles.Contains(Roles.User))),
            Times.Once);
        _userManager.Verify(x => x.AddToRoleAsync(applicationUser, Roles.Printer), Times.Once);
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
