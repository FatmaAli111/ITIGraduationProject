using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Service.Identity.Authantication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Test.Features.Identity
{

    [TestFixture]
    public class IdentityServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<SignInManager<ApplicationUser>> _signInManager;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<IEmailService> _emailService;
        private Mock<IConfiguration> _configuration;
        private Mock<IJwtService> _jwtService;
        private Mock<IHttpContextAccessor> _httpContextAccessor;

        private IdentityService _service;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>
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

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            _signInManager = new Mock<SignInManager<ApplicationUser>>
            (
                _userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null,
                null,
                null,
                null
            );

            _unitOfWork = new Mock<IUnitOfWork>();
            _emailService = new Mock<IEmailService>();
            _configuration = new Mock<IConfiguration>();
            _jwtService = new Mock<IJwtService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _service = new IdentityService(
                _userManager.Object,
                _unitOfWork.Object,
                _emailService.Object,
                _configuration.Object,
                _jwtService.Object,
                _httpContextAccessor.Object,
                _signInManager.Object);
        }

        [Test]
        public async Task RegisterAsync_Should_Return_BadRequest_When_Email_Exists()
        {
            var request = new RegisterRequestDTO
            {
                Name = "Fatma",
                Email = "fatma@test.com",
                Password = "123456"
            };

            _userManager
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new ApplicationUser());

            var result = await _service.RegisterAsync(request);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email already exists."));
        }

        [Test]
        public async Task ConfirmEmailAsync_Should_Return_NotFound_When_User_Not_Exists()
        {
            _userManager
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _service.ConfirmEmailAsync(
                Guid.NewGuid().ToString(),
                "token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LoginAsync_Should_Return_Unauthorized_When_User_Not_Exists()
        {
            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _service.LoginAsync(
                new LoginRequestDTO
                {
                    Email = "test@test.com",
                    Password = "123456"
                });

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LoginAsync_Should_Return_BadRequest_When_Email_Not_Confirmed()
        {
            var user = new ApplicationUser
            {
                EmailConfirmed = false
            };

            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var result = await _service.LoginAsync(
                new LoginRequestDTO
                {
                    Email = "test@test.com",
                    Password = "123456"
                });

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task ForgetPasswordAsync_Should_Return_BadRequest_When_User_Not_Found()
        {
            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _service.ForgetPasswordAsync(
                "test@test.com");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task ResetPasswordAsync_Should_Return_NotFound_When_User_Not_Found()
        {
            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _service.ResetPasswordAsync(
                "test@test.com",
                "token",
                "NewPassword123");

            Assert.That(result.Succeeded, Is.False);
        }
    
[Test]
        public async Task RefreshTokenAsync_Should_Return_Unauthorized_When_Token_Not_Found()
        {
            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken)null);

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.RefreshTokenAsync("invalid-token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task RefreshTokenAsync_Should_Return_Unauthorized_When_Token_Revoked()
        {
            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken
                {
                    IsRevoked = true,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                });

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.RefreshTokenAsync("token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task RefreshTokenAsync_Should_Return_Unauthorized_When_Token_Expired()
        {
            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken
                {
                    IsRevoked = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(-1)
                });

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.RefreshTokenAsync("token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LogoutAsync_Should_Return_Unauthorized_When_Token_Not_Found()
        {
            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken)null);

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.LogoutAsync("token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LogoutAsync_Should_Revoke_Token()
        {
            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "token",
                IsRevoked = false
            };

            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetByTokenAsync("token"))
                .ReturnsAsync(token);

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.LogoutAsync("token");

            Assert.That(result.Succeeded, Is.True);
            Assert.That(token.IsRevoked, Is.True);

            refreshRepo.Verify(
                x => x.Update(It.IsAny<RefreshToken>()),
                Times.Once);
        }

        [Test]
        public async Task LogoutAllDevicesAsync_Should_Return_Unauthorized_When_User_Not_Found()
        {
            var context = new DefaultHttpContext();

            _httpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(context);

            var result = await _service.LogoutAllDevicesAsync();

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task LogoutAllDevicesAsync_Should_Revoke_All_User_Tokens()
        {
            var userId = Guid.NewGuid();

            var context = new DefaultHttpContext();

            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
                }));

            _httpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(context);

            var tokens = new List<RefreshToken>
    {
        new()
        {
            Token = "1",
            IsRevoked = false
        },
        new()
        {
            Token = "2",
            IsRevoked = false
        }
    };

            var refreshRepo = new Mock<IRefreshTokenRepository>();

            refreshRepo
                .Setup(x => x.GetUserTokensAsync(userId))
                .ReturnsAsync(tokens);

            _unitOfWork
                .Setup(x => x.RefreshTokens)
                .Returns(refreshRepo.Object);

            var result = await _service.LogoutAllDevicesAsync();

            Assert.That(result.Succeeded, Is.True);

            Assert.That(tokens.All(x => x.IsRevoked));
        }

        [Test]
        public async Task ConfirmEmailAsync_Should_Return_BadRequest_When_Invalid_UserId()
        {
            var result = await _service.ConfirmEmailAsync(
                "invalid-guid",
                "token");

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task ConfirmEmailAsync_Should_Return_Success_When_Email_Already_Confirmed()
        {
            var user = new ApplicationUser
            {
                EmailConfirmed = true
            };

            _userManager
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var result = await _service.ConfirmEmailAsync(
                Guid.NewGuid().ToString(),
                "token");

            Assert.That(result.Succeeded, Is.True);
        } }
    }