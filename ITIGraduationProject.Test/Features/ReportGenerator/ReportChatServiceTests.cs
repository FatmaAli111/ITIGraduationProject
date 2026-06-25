using FluentAssertions;
using Moq;
using NUnit.Framework;
using ITIGraduationProject.Service.ReportGenerator;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using ITIGraduationProject.Application.Interfaces.IRepositories;

namespace ITIGraduationProject.Tests.Services;

[TestFixture]
public class ReportChatServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IAiChatSessionRepository> _sessionRepoMock;
    private Mock<IAiChatMessageRepository> _messageRepoMock;
    private Mock<ILangflowService> _langflowMock;
    private Mock<ICurrentUserService> _currentUserMock;

    private ReportChatService _service;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sessionRepoMock = new Mock<IAiChatSessionRepository>();
        _messageRepoMock = new Mock<IAiChatMessageRepository>();
        _langflowMock = new Mock<ILangflowService>();
        _currentUserMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock
            .Setup(x => x.AiChatSessions)
            .Returns(_sessionRepoMock.Object);

        _unitOfWorkMock
            .Setup(x => x.AiChatMessages)
            .Returns(_messageRepoMock.Object);

        _service = new ReportChatService(
            _unitOfWorkMock.Object,
            _langflowMock.Object,
            _currentUserMock.Object);
    }

    [Test]
    public async Task CreateSessionAsync_Should_Create_New_Session()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock
            .Setup(x => x.UserId)
            .Returns(userId);

        _sessionRepoMock
            .Setup(x => x.AddAsync(It.IsAny<AiChatSession>()))
            .ReturnsAsync((AiChatSession s) => s);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateSessionAsync();

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _sessionRepoMock.Verify(
            x => x.AddAsync(It.IsAny<AiChatSession>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Test]
    public async Task SendMessageAsync_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _sessionRepoMock
            .Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((AiChatSession?)null);

        // Act
        var result =
            await _service.SendMessageAsync(
                sessionId,
                "hello");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Session not found");
    }

    [Test]
    public async Task SendMessageAsync_Should_Save_User_And_AI_Messages()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        var session = new AiChatSession
        {
            Id = sessionId,
            SessionType = AiChatSessionType.ReportGeneration
        };

        _sessionRepoMock
            .Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);

        _messageRepoMock
            .Setup(x => x.AddAsync(It.IsAny<AiChatMessage>()))
            .ReturnsAsync((AiChatMessage m) => m);

        _langflowMock
            .Setup(x => x.SendMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("AI Response");

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result =
            await _service.SendMessageAsync(
                sessionId,
                "hello");

        // Assert
        result.Succeeded.Should().BeTrue();

        result.Data.Response
            .Should()
            .Be("AI Response");

        _messageRepoMock.Verify(
            x => x.AddAsync(It.IsAny<AiChatMessage>()),
            Times.Exactly(2));

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Test]
    public async Task GetHistoryAsync_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _sessionRepoMock
            .Setup(x => x.GetWithMessagesAsync(sessionId))
            .ReturnsAsync((AiChatSession?)null);

        // Act
        var result =
            await _service.GetHistoryAsync(sessionId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Session not found");
    }

    [Test]
    public async Task GetHistoryAsync_Should_Return_Messages()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        var session = new AiChatSession
        {
            Id = sessionId,
            AiChatMessages = new List<AiChatMessage>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Sender = "user",
                    MessageText = "Hello"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Sender = "ai",
                    MessageText = "Hi"
                }
            }
        };

        _sessionRepoMock
            .Setup(x => x.GetWithMessagesAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result =
            await _service.GetHistoryAsync(sessionId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Count.Should().Be(2);
    }
}