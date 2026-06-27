using ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Handlers;
using ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ITIGraduationProject.Test.Features.PrinterDashboard;

[TestFixture]
public class PrinterDashboardCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private UpdateOrderItemStatusCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();

        _handler = new UpdateOrderItemStatusCommandHandler(
            _uow.Object,
            _currentUser.Object);
    }

    [Test]
    public async Task UpdateOrderItemStatus_Should_Succeed()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new()
            {
                Id = printerProfileId,
                UserId = userId,
                IsActive = true
            }
        };

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            OrderId = orderId,
            PrinterProfileId = printerProfileId,
            Status = OrderItemStatus.AssignedToPrinter,
            SnapshotImageURL = "snap.png",
            Quantity = 2
        };

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "WLY-2026-11111"
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItemId))
            .ReturnsAsync(orderItem);

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);
        _uow.Setup(x => x.Orders).Returns(orderRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateOrderItemStatusCommand
        {
            Id = orderItemId,
            NewStatus = OrderItemStatus.InProduction
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.Id, Is.EqualTo(orderItemId));
        Assert.That(result.Data.OrderNumber, Is.EqualTo("WLY-2026-11111"));
        Assert.That(result.Data.Status, Is.EqualTo(OrderItemStatus.InProduction.ToString()));
        Assert.That(orderItem.Status, Is.EqualTo(OrderItemStatus.InProduction));

        orderItemRepo.Verify(x => x.Update(orderItem), Times.Once);
    }

    [Test]
    public async Task UpdateOrderItemStatus_Should_Return_NotFound_When_No_Printer_Profile()
    {
        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<PrinterProfile>().AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var command = new UpdateOrderItemStatusCommand
        {
            Id = Guid.NewGuid(),
            NewStatus = OrderItemStatus.InProduction
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(result.Message, Is.EqualTo("No printer profile found for this user. Create one first."));
    }

    [Test]
    public async Task UpdateOrderItemStatus_Should_Return_NotFound_When_OrderItem_NotFound()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = Guid.NewGuid(), UserId = userId }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((OrderItem?)null);

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var command = new UpdateOrderItemStatusCommand
        {
            Id = Guid.NewGuid(),
            NewStatus = OrderItemStatus.InProduction
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(result.Message, Is.EqualTo("Order item not found."));
    }

    [Test]
    public async Task UpdateOrderItemStatus_Should_Return_Unauthorized_When_Different_Printer()
    {
        var userId = Guid.NewGuid();
        var myPrinterProfileId = Guid.NewGuid();
        var otherPrinterProfileId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = myPrinterProfileId, UserId = userId }
        };

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            PrinterProfileId = otherPrinterProfileId,
            Status = OrderItemStatus.AssignedToPrinter
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItemId))
            .ReturnsAsync(orderItem);

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var command = new UpdateOrderItemStatusCommand
        {
            Id = orderItemId,
            NewStatus = OrderItemStatus.InProduction
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(result.Message, Is.EqualTo("You can only update order items assigned to you."));
    }

    [Test]
    public async Task UpdateOrderItemStatus_Should_Return_BadRequest_When_Reverting_Status()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = printerProfileId, UserId = userId }
        };

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            PrinterProfileId = printerProfileId,
            Status = OrderItemStatus.InProduction
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItemId))
            .ReturnsAsync(orderItem);

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var command = new UpdateOrderItemStatusCommand
        {
            Id = orderItemId,
            NewStatus = OrderItemStatus.AssignedToPrinter
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Cannot revert to a previous status."));
    }
}
