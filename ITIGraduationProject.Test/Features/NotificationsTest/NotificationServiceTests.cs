using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using ITIGraduationProject.Service.NotificationServices;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Test.Features.NotificationsTest
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<INotificationSender> _notificationSender;
        private Mock<IUnitOfWork> _unitOfWork;

        private NotificationService _service;

        [SetUp]
        public void Setup()
        {
            _notificationSender = new Mock<INotificationSender>();
            _unitOfWork = new Mock<IUnitOfWork>();

            _service = new NotificationService(
                _notificationSender.Object,
                _unitOfWork.Object);
        }

        [Test]
        public async Task GetNotificationsAsync_Should_Return_Notifications()
        {
            var userId = Guid.NewGuid();

            var notifications = new List<Notification>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Title",
                    Message = "Message"
                }
            };

            var repo = new Mock<INotificationRepository>();

            repo.Setup(x =>
                    x.GetByUserAsync(userId))
                .ReturnsAsync(notifications);

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.GetNotificationsAsync(userId);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUnreadNotificationsAsync_Should_Return_Unread()
        {
            var userId = Guid.NewGuid();

            var notifications = new List<Notification>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsRead = false
                }
            };

            var repo = new Mock<INotificationRepository>();

            repo.Setup(x =>
                    x.GetUnreadByUserAsync(userId))
                .ReturnsAsync(notifications);

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.GetUnreadNotificationsAsync(userId);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task MarkAsReadAsync_Should_Return_NotFound_When_Notification_Not_Exists()
        {
            var repo = new Mock<INotificationRepository>();

            repo.Setup(x =>
                    x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Notification)null);

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.MarkAsReadAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid());

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task MarkAsReadAsync_Should_Return_Unauthorized_When_User_Not_Owner()
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            var repo = new Mock<INotificationRepository>();

            repo.Setup(x =>
                    x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(notification);

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.MarkAsReadAsync(
                    Guid.NewGuid(),
                    notification.Id);

            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        public async Task MarkAsReadAsync_Should_Mark_Notification_As_Read()
        {
            var userId = Guid.NewGuid();

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsRead = false
            };

            var repo = new Mock<INotificationRepository>();

            repo.Setup(x =>
                    x.GetByIdAsync(notification.Id))
                .ReturnsAsync(notification);

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.MarkAsReadAsync(
                    userId,
                    notification.Id);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(notification.IsRead, Is.True);

            repo.Verify(
                x => x.Update(It.IsAny<Notification>()),
                Times.Once);
        }

        [Test]
        public async Task SendNotificationAsync_Should_Create_Notification()
        {
            var repo = new Mock<INotificationRepository>();

            _unitOfWork.Setup(x => x.Notifications)
                .Returns(repo.Object);

            var result =
                await _service.SendNotificationAsync(
                    Guid.NewGuid(),
                    "Title",
                    "Message",
                    NotificationType.SystemAlert);

            Assert.That(result.Succeeded, Is.True);

            repo.Verify(
                x => x.AddAsync(It.IsAny<Notification>()),
                Times.Once);

            _notificationSender.Verify(
                x => x.SendToUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTime>()),
                Times.Once);
        }

    //    [Test]
    //    public async Task MarkAllAsReadAsync_Should_Mark_All_Unread()
    //    {
    //        var userId = Guid.NewGuid();

    //        var notifications = new List<Notification>
    //        {
    //            new()
    //            {
    //                UserId = userId,
    //                IsRead = false
    //            },
    //            new()
    //            {
    //                UserId = userId,
    //                IsRead = false
    //            }
    //        };

    //        var mockQueryable =
    //            notifications
    //            .AsQueryable()
    //            .BuildMockDbSet();

    //        var repo = new Mock<INotificationRepository>();

    //        repo.Setup(x => x.GetTableAsTracking())
    //            .Returns(mockQueryable.Object);

    //        _unitOfWork.Setup(x => x.Notifications)
    //            .Returns(repo.Object);

    //        var result =
    //            await _service.MarkAllAsReadAsync(userId);

    //        Assert.That(result.Succeeded, Is.True);

    //        Assert.That(
    //            notifications.All(x => x.IsRead),
    //            Is.True);
    //    }
    }

}
