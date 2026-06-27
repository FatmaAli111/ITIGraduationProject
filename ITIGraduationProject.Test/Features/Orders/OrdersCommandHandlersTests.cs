using ITIGraduationProject.Application.DTOS.Orders;
using ITIGraduationProject.Application.Features.Orders.Commands.Handlers;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ITIGraduationProject.Test.Features.Orders;

[TestFixture]
public class OrdersCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IPaymentService> _paymentService = null!;
    private Mock<INotificationService> _notificationService = null!;

    private CreateOrderCommandHandler _createOrderHandler = null!;
    private CancelOrderCommandHandler _cancelOrderHandler = null!;
    private UpdateOrderStatusCommandHandler _updateOrderStatusHandler = null!;
    private AssignPrinterToOrderItemCommandHandler _assignPrinterHandler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _paymentService = new Mock<IPaymentService>();
        _notificationService = new Mock<INotificationService>();

        _createOrderHandler = new CreateOrderCommandHandler(
            _uow.Object,
            _paymentService.Object,
            _notificationService.Object);

        _cancelOrderHandler = new CancelOrderCommandHandler(_uow.Object);
        _updateOrderStatusHandler = new UpdateOrderStatusCommandHandler(_uow.Object);
        _assignPrinterHandler = new AssignPrinterToOrderItemCommandHandler(_uow.Object);
    }

    #region CreateOrder

    [Test]
    public async Task CreateOrder_Should_Succeed()
    {
        var userId = Guid.NewGuid();
        var designId = Guid.NewGuid();

        var design = new Design
        {
            Id = designId,
            CalculatedPrice = 100m,
            SnapshotImageURL = "snap.png",
            SelectedFabric = FabricType.Cotton,
            SelectedColor = "Blue"
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(designId))
            .ReturnsAsync(design);

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        _uow.Setup(x => x.Designs).Returns(designRepo.Object);
        _uow.Setup(x => x.Orders).Returns(orderRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _paymentService.Setup(x => x.CreatePaymentSessionAsync(It.IsAny<Order>()))
            .ReturnsAsync("https://pay.example.com/session");

        _notificationService.Setup(x => x.SendNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>()))
            .ReturnsAsync(new ITIGraduationProject.Application.Bases.Response<bool> { Succeeded = true });

        var command = new CreateOrderCommand
        {
            UserId = userId,
            ReceiverName = "Ahmed",
            PhoneNumber = "01000000000",
            Address = "123 Street",
            City = "Cairo",
            OrderItems = new List<CreateOrderItemDTO>
            {
                new() { DesignId = designId, Quantity = 2 }
            }
        };

        var result = await _createOrderHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.EqualTo("https://pay.example.com/session"));

        orderRepo.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
        _paymentService.Verify(x => x.CreatePaymentSessionAsync(It.IsAny<Order>()), Times.Once);
    }

    [Test]
    public async Task CreateOrder_Should_Return_BadRequest_When_Design_NotFound()
    {
        var designId = Guid.NewGuid();

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(designId))
            .ReturnsAsync((Design?)null);

        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            ReceiverName = "Ahmed",
            PhoneNumber = "01000000000",
            Address = "123 Street",
            City = "Cairo",
            OrderItems = new List<CreateOrderItemDTO>
            {
                new() { DesignId = designId, Quantity = 1 }
            }
        };

        var result = await _createOrderHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo($"Design with ID {designId} does not exist."));
    }

    [Test]
    public async Task CreateOrder_Should_Return_BadRequest_When_Save_Fails()
    {
        var designId = Guid.NewGuid();

        var design = new Design
        {
            Id = designId,
            CalculatedPrice = 50m,
            SnapshotImageURL = "snap.png"
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(designId))
            .ReturnsAsync(design);

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        _uow.Setup(x => x.Designs).Returns(designRepo.Object);
        _uow.Setup(x => x.Orders).Returns(orderRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            ReceiverName = "Ahmed",
            PhoneNumber = "01000000000",
            Address = "123 Street",
            City = "Cairo",
            OrderItems = new List<CreateOrderItemDTO>
            {
                new() { DesignId = designId, Quantity = 1 }
            }
        };

        var result = await _createOrderHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Failed to place the order. Please try again."));
    }

    #endregion

    #region CancelOrder

    [Test]
    public async Task CancelOrder_Should_Succeed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Pending
        };

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _uow.Setup(x => x.Orders).Returns(orderRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _cancelOrderHandler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Cancelled));

        orderRepo.Verify(x => x.Update(order), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CancelOrder_Should_Return_NotFound()
    {
        _uow.Setup(x => x.Orders.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var result = await _cancelOrderHandler.Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(result.Message, Is.EqualTo("Order not found."));
    }

    [TestCase(OrderStatus.Shipped)]
    [TestCase(OrderStatus.Delivered)]
    public async Task CancelOrder_Should_Return_BadRequest_When_Invalid_State(OrderStatus status)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = status
        };

        _uow.Setup(x => x.Orders.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var result = await _cancelOrderHandler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Cannot cancel an order that has already been shipped or delivered."));
    }

    #endregion

    #region UpdateOrderStatus

    [Test]
    public async Task UpdateOrderStatus_Should_Succeed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Pending
        };

        _uow.Setup(x => x.Orders.GetByIdAsync(order.Id))
            .ReturnsAsync(order);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = order.Id,
            NewStatus = OrderStatus.Processing
        };

        var result = await _updateOrderStatusHandler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.True);
        Assert.That(order.OrderStatus, Is.EqualTo(OrderStatus.Processing));
    }

    [Test]
    public async Task UpdateOrderStatus_Should_Return_False_When_NotFound()
    {
        _uow.Setup(x => x.Orders.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            NewStatus = OrderStatus.Processing
        };

        var result = await _updateOrderStatusHandler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    #endregion

    #region AssignPrinterToOrderItem

    [Test]
    public async Task AssignPrinterToOrderItem_Should_Succeed()
    {
        var orderItemId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            Status = OrderItemStatus.Pending
        };

        var printerProfile = new PrinterProfile
        {
            Id = printerProfileId,
            IsActive = true
        };

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItemId))
            .ReturnsAsync(orderItem);

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetByIdAsync(printerProfileId))
            .ReturnsAsync(printerProfile);

        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);
        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new AssignPrinterToOrderItemCommand
        {
            OrderItemId = orderItemId,
            PrinterProfileId = printerProfileId
        };

        var result = await _assignPrinterHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.OrderItemId, Is.EqualTo(orderItemId));
        Assert.That(result.Data.PrinterProfileId, Is.EqualTo(printerProfileId));
        Assert.That(result.Data.Status, Is.EqualTo(OrderItemStatus.AssignedToPrinter));
        Assert.That(orderItem.PrinterProfileId, Is.EqualTo(printerProfileId));

        orderItemRepo.Verify(x => x.Update(orderItem), Times.Once);
    }

    [Test]
    public async Task AssignPrinterToOrderItem_Should_Return_NotFound_When_OrderItem_NotFound()
    {
        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((OrderItem?)null);

        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var command = new AssignPrinterToOrderItemCommand
        {
            OrderItemId = Guid.NewGuid(),
            PrinterProfileId = Guid.NewGuid()
        };

        var result = await _assignPrinterHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(result.Message, Is.EqualTo("Order item not found."));
    }

    [Test]
    public async Task AssignPrinterToOrderItem_Should_Return_BadRequest_When_Printer_NotFound()
    {
        var orderItem = new OrderItem { Id = Guid.NewGuid() };

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItem.Id))
            .ReturnsAsync(orderItem);

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PrinterProfile?)null);

        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);
        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var command = new AssignPrinterToOrderItemCommand
        {
            OrderItemId = orderItem.Id,
            PrinterProfileId = Guid.NewGuid()
        };

        var result = await _assignPrinterHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Printer profile not found or inactive."));
    }

    [Test]
    public async Task AssignPrinterToOrderItem_Should_Return_BadRequest_When_Printer_Inactive()
    {
        var orderItem = new OrderItem { Id = Guid.NewGuid() };
        var printerProfileId = Guid.NewGuid();

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetByIdAsync(orderItem.Id))
            .ReturnsAsync(orderItem);

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetByIdAsync(printerProfileId))
            .ReturnsAsync(new PrinterProfile { Id = printerProfileId, IsActive = false });

        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);
        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var command = new AssignPrinterToOrderItemCommand
        {
            OrderItemId = orderItem.Id,
            PrinterProfileId = printerProfileId
        };

        var result = await _assignPrinterHandler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Printer profile not found or inactive."));
    }

    #endregion
}
