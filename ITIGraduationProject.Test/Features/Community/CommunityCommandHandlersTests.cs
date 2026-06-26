using ITIGraduationProject.Application.Features.Community.Commands.Handlers;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Identity;
using System.Net;
using ITIGraduationProject.Domain.Enums;
using ITIGraduationProject.Application.Repositories;
using MockQueryable.Moq;

namespace ITIGraduationProject.Test.Features.Community
{
    public class CommunityCommandHandlersTests
    {
        private Mock<IUnitOfWork> _uow;
        private Mock<ICurrentUserService> _currentUser;
        private Mock<INotificationService> _notificationService;

        private Mock<ILogger<AddCommentCommandHandler>> _addCommentLogger;
        private Mock<ILogger<ToggleLikeCommandHandler>> _toggleLikeLogger;
        private Mock<ILogger<ResolveModerationReportCommandHandler>> _resolveLogger;

        private AddCommentCommandHandler _addCommentHandler;
        private DeleteCommentCommandHandler _deleteCommentHandler;
        private ReportTemplateCommandHandler _reportTemplateHandler;
        private ToggleLikeCommandHandler _toggleLikeHandler;
        private ToggleSaveCommandHandler _toggleSaveHandler;
        private ResolveModerationReportCommandHandler _resolveHandler;

        private AddCommentCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _uow = new Mock<IUnitOfWork>();
            _currentUser = new Mock<ICurrentUserService>();
            _notificationService = new Mock<INotificationService>();

            _addCommentLogger = new Mock<ILogger<AddCommentCommandHandler>>();
            _toggleLikeLogger = new Mock<ILogger<ToggleLikeCommandHandler>>();
            _resolveLogger = new Mock<ILogger<ResolveModerationReportCommandHandler>>();

            _currentUser
                .Setup(x => x.UserId)
                .Returns(Guid.NewGuid());

            _addCommentHandler = new AddCommentCommandHandler(
                _uow.Object,
                _currentUser.Object,
                _notificationService.Object,
                _addCommentLogger.Object);

            _deleteCommentHandler = new DeleteCommentCommandHandler(
                _uow.Object,
                _currentUser.Object);

            _reportTemplateHandler = new ReportTemplateCommandHandler(
                _uow.Object,
                _currentUser.Object);

            _toggleSaveHandler = new ToggleSaveCommandHandler(
                _uow.Object,
                _currentUser.Object);

            _toggleLikeHandler = new ToggleLikeCommandHandler(
                _uow.Object,
                _currentUser.Object,
                _notificationService.Object,
                _toggleLikeLogger.Object);

            _resolveHandler = new ResolveModerationReportCommandHandler(
                _uow.Object,
                _notificationService.Object,
                _resolveLogger.Object);
            _handler = new AddCommentCommandHandler(
                _uow.Object,
                _currentUser.Object,
                _notificationService.Object,
                _addCommentLogger.Object);
        }
        [Test]
        public async Task AddComment_Should_Return_NotFound_When_Template_NotFound()
        {
            // Arrange
            var command = new AddCommentCommand(
                Guid.NewGuid(),
                "Hello"
            );

            _uow.Setup(x => x.Templates.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Template?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Template not found"));
        }
        [Test]
        public async Task AddComment_Should_Create_Comment()
        {
            var currentUserId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId)
                .Returns(currentUserId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                Name = "Template 1",
                CreatorUserId = Guid.NewGuid(),
                IsDeleted = false
            };

            var user = new User
            {
                Id = currentUserId,
                Name = "Fatma"
            };

            var repo = new Mock<ICommunityInteractionRepository>();

            repo.Setup(x => x.AddAsync(It.IsAny<CommunityInteraction>()))
                .ReturnsAsync((CommunityInteraction x) => x);

            _uow.Setup(x => x.CommunityInteractions)
                .Returns(repo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.Users.GetByIdAsync(currentUserId))
                .ReturnsAsync(user);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new AddCommentCommand(
      template.Id,
      "Excellent"
  );
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Content, Is.EqualTo("Excellent"));

            repo.Verify(x =>
                x.AddAsync(It.IsAny<CommunityInteraction>()),
                Times.Once);

            _uow.Verify(x =>
                x.SaveChangesAsync(),
                Times.Once);
        }


        [Test]
        public async Task ReportTemplate_Should_Return_NotFound_When_Template_NotFound()
        {
            _uow.Setup(x => x.Templates.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Template?)null);

            var command = new ReportTemplateCommand(
                Guid.NewGuid(),
                "Spam");

            var result = await _reportTemplateHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Template not found"));
        }

        [Test]
        public async Task ReportTemplate_Should_Create_Report()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId)
                .Returns(userId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                IsDeleted = false
            };

            var repo = new Mock<IModerationReportRepository>();

            repo.Setup(x => x.AddAsync(It.IsAny<ModerationReport>()))
                .ReturnsAsync((ModerationReport r) => r);

            _uow.Setup(x => x.ModerationReports)
                .Returns(repo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new ReportTemplateCommand(
                template.Id,
                "Spam");

            var result = await _reportTemplateHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            repo.Verify(x =>
                x.AddAsync(It.IsAny<ModerationReport>()),
                Times.Once);

            _uow.Verify(x =>
                x.SaveChangesAsync(),
                Times.Once);
        }
        [Test]
        public async Task DeleteComment_Should_Return_NotFound_When_Comment_NotFound()
        {
            _uow.Setup(x => x.CommunityInteractions.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((CommunityInteraction?)null);

            var command = new DeleteCommentCommand(Guid.NewGuid());

            var result = await _deleteCommentHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Comment not found"));
        }

        [Test]
        public async Task DeleteComment_Should_Return_Unauthorized_When_User_Is_Not_Owner()
        {
            var comment = new CommunityInteraction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                InteractionType = InteractionType.Comment,
                IsDeleted = false
            };

            _currentUser.Setup(x => x.UserId)
                .Returns(Guid.NewGuid());

            _uow.Setup(x => x.CommunityInteractions.GetByIdAsync(comment.Id))
                .ReturnsAsync(comment);

            var command = new DeleteCommentCommand(comment.Id);

            var result = await _deleteCommentHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task DeleteComment_Should_Delete_Comment()
        {
            var userId = Guid.NewGuid();

            var comment = new CommunityInteraction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                InteractionType = InteractionType.Comment,
                IsDeleted = false
            };

            _currentUser.Setup(x => x.UserId)
                .Returns(userId);

            _uow.Setup(x => x.CommunityInteractions.GetByIdAsync(comment.Id))
                .ReturnsAsync(comment);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new DeleteCommentCommand(comment.Id);

            var result = await _deleteCommentHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(comment.IsDeleted, Is.True);

            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        #region ToggleLike

        [Test]
        public async Task ToggleLike_Should_Return_NotFound_When_Template_NotFound()
        {
            _uow.Setup(x => x.Templates.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Template?)null);

            var command = new ToggleLikeCommand(Guid.NewGuid());

            var result = await _toggleLikeHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Template not found"));
        }

        [Test]
        public async Task ToggleLike_Should_Add_Like()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId).Returns(userId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                CreatorUserId = Guid.NewGuid(),
                Name = "Test",
                LikesCount = 0,
                IsDeleted = false
            };

            var interactionRepo = new Mock<ICommunityInteractionRepository>();

            interactionRepo
                .Setup(x => x.GetTableAsTracking())
                .Returns(new List<CommunityInteraction>().AsQueryable().BuildMock());

            interactionRepo
                .Setup(x => x.AddAsync(It.IsAny<CommunityInteraction>()))
                .ReturnsAsync((CommunityInteraction x) => x);

            _uow.Setup(x => x.CommunityInteractions)
                .Returns(interactionRepo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.Users.GetByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Name = "Fatma"
                });

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new ToggleLikeCommand(template.Id);

            var result = await _toggleLikeHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Liked, Is.True);
            Assert.That(result.Data.Count, Is.EqualTo(1));

            interactionRepo.Verify(x =>
                x.AddAsync(It.IsAny<CommunityInteraction>()),
                Times.Once);
        }

        [Test]
        public async Task ToggleLike_Should_Remove_Like()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId).Returns(userId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                LikesCount = 5,
                IsDeleted = false
            };

            var like = new CommunityInteraction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TemplateId = template.Id,
                InteractionType = InteractionType.Like
            };

            var repo = new Mock<ICommunityInteractionRepository>();

            repo.Setup(x => x.GetTableAsTracking())
                .Returns(new List<CommunityInteraction>
                {
            like
                }.AsQueryable().BuildMock());

            _uow.Setup(x => x.CommunityInteractions)
                .Returns(repo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new ToggleLikeCommand(template.Id);

            var result = await _toggleLikeHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Liked, Is.False);
            Assert.That(result.Data.Count, Is.EqualTo(4));

            repo.Verify(x => x.Update(It.IsAny<CommunityInteraction>()), Times.Once);
        }

        #endregion


        #region ToggleSave

        [Test]
        public async Task ToggleSave_Should_Return_NotFound_When_Template_NotFound()
        {
            _uow.Setup(x => x.Templates.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Template?)null);

            var command = new ToggleSaveCommand(Guid.NewGuid());

            var result = await _toggleSaveHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Template not found"));
        }

        [Test]
        public async Task ToggleSave_Should_Save_Template()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId).Returns(userId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                IsDeleted = false
            };

            var repo = new Mock<ICommunityInteractionRepository>();

            repo.Setup(x => x.GetTableAsTracking())
                .Returns(new List<CommunityInteraction>().AsQueryable().BuildMock());

            repo.Setup(x => x.AddAsync(It.IsAny<CommunityInteraction>()))
                .ReturnsAsync((CommunityInteraction x) => x);

            _uow.Setup(x => x.CommunityInteractions)
                .Returns(repo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new ToggleSaveCommand(template.Id);

            var result = await _toggleSaveHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Saved, Is.True);

            repo.Verify(x =>
                x.AddAsync(It.IsAny<CommunityInteraction>()),
                Times.Once);
        }

        [Test]
        public async Task ToggleSave_Should_UnSave_Template()
        {
            var userId = Guid.NewGuid();

            _currentUser.Setup(x => x.UserId).Returns(userId);

            var template = new Template
            {
                Id = Guid.NewGuid(),
                IsDeleted = false
            };

            var save = new CommunityInteraction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TemplateId = template.Id,
                InteractionType = InteractionType.Save
            };

            var repo = new Mock<ICommunityInteractionRepository>();

            repo.Setup(x => x.GetTableAsTracking())
                .Returns(new List<CommunityInteraction>
                {
            save
                }.AsQueryable().BuildMock());

            _uow.Setup(x => x.CommunityInteractions)
                .Returns(repo.Object);

            _uow.Setup(x => x.Templates.GetByIdAsync(template.Id))
                .ReturnsAsync(template);

            _uow.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new ToggleSaveCommand(template.Id);

            var result = await _toggleSaveHandler.Handle(command, CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Saved, Is.False);

            repo.Verify(x =>
                x.Update(It.IsAny<CommunityInteraction>()),
                Times.Once);
        }

        #endregion

    }
}
