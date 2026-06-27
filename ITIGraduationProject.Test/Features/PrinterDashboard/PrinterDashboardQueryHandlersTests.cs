using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Handlers;
using ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.PrinterDashboard;

[TestFixture]
public class PrinterDashboardQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;

    private GetMyPrinterProfileSummaryQueryHandler _summaryHandler = null!;
    private GetMyAssignedOrderItemsQueryHandler _assignedItemsHandler = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();

        _summaryHandler = new GetMyPrinterProfileSummaryQueryHandler(
            _uow.Object,
            _currentUser.Object);

        _assignedItemsHandler = new GetMyAssignedOrderItemsQueryHandler(
            _uow.Object,
            _currentUser.Object);
    }

    #region GetMyPrinterProfileSummary

    [Test]
    public async Task GetMyPrinterProfileSummary_Should_Return_Data()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new()
            {
                Id = printerProfileId,
                UserId = userId,
                SupportedFabrics = FabricType.Cotton,
                SupportedPrintMethods = PrintMethodType.DirectToGarment,
                IsActive = true
            }
        };

        var orderItems = new List<OrderItem>
        {
            new()
            {
                PrinterProfileId = printerProfileId,
                Status = OrderItemStatus.AssignedToPrinter
            },
            new()
            {
                PrinterProfileId = printerProfileId,
                Status = OrderItemStatus.InProduction
            },
            new()
            {
                PrinterProfileId = printerProfileId,
                Status = OrderItemStatus.Shipped
            }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetTableNoTracking())
            .Returns(orderItems.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var result = await _summaryHandler.Handle(new GetMyPrinterProfileSummaryQuery(), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.ProfileId, Is.EqualTo(printerProfileId));
        Assert.That(result.Data.TotalAssignedItems, Is.EqualTo(3));
        Assert.That(result.Data.PendingItems, Is.EqualTo(2));
        Assert.That(result.Data.CompletedItems, Is.EqualTo(1));
        Assert.That(result.Data.SupportedFabrics, Is.EqualTo(FabricType.Cotton));
    }

    [Test]
    public async Task GetMyPrinterProfileSummary_Should_Return_NotFound()
    {
        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<PrinterProfile>().AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var result = await _summaryHandler.Handle(new GetMyPrinterProfileSummaryQuery(), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("No printer profile found for this user. Create one first."));
    }

    #endregion

    #region GetMyAssignedOrderItems

    [Test]
    public async Task GetMyAssignedOrderItems_Should_Return_Data()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = printerProfileId, UserId = userId }
        };

        var orderItems = new List<OrderItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                PrinterProfileId = printerProfileId,
                SnapshotImageURL = "snap-1.png",
                Quantity = 1,
                Status = OrderItemStatus.AssignedToPrinter,
                Order = new Order
                {
                    OrderNumber = "WLY-2026-10001",
                    CreatedAt = DateTime.UtcNow
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                PrinterProfileId = printerProfileId,
                SnapshotImageURL = "snap-2.png",
                Quantity = 3,
                Status = OrderItemStatus.InProduction,
                Order = new Order
                {
                    OrderNumber = "WLY-2026-10002",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetTableNoTracking())
            .Returns(orderItems.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var result = await _assignedItemsHandler.Handle(
            new GetMyAssignedOrderItemsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetMyAssignedOrderItems_Should_Return_Filtered_Results()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = printerProfileId, UserId = userId }
        };

        var orderItems = new List<OrderItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PrinterProfileId = printerProfileId,
                Status = OrderItemStatus.AssignedToPrinter,
                Quantity = 1,
                SnapshotImageURL = "snap.png",
                Order = new Order { OrderNumber = "WLY-1", CreatedAt = DateTime.UtcNow }
            },
            new()
            {
                Id = Guid.NewGuid(),
                PrinterProfileId = printerProfileId,
                Status = OrderItemStatus.Shipped,
                Quantity = 2,
                SnapshotImageURL = "snap.png",
                Order = new Order { OrderNumber = "WLY-2", CreatedAt = DateTime.UtcNow }
            }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetTableNoTracking())
            .Returns(orderItems.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var result = await _assignedItemsHandler.Handle(
            new GetMyAssignedOrderItemsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Status = OrderItemStatus.AssignedToPrinter
            },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].Status, Is.EqualTo(OrderItemStatus.AssignedToPrinter.ToString()));
    }

    [Test]
    public async Task GetMyAssignedOrderItems_Should_Return_Empty_When_No_Items()
    {
        var userId = Guid.NewGuid();
        var printerProfileId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfiles = new List<PrinterProfile>
        {
            new() { Id = printerProfileId, UserId = userId }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(printerProfiles.AsQueryable().BuildMock());

        var orderItemRepo = new Mock<IOrderItemRepository>();
        orderItemRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<OrderItem>().AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.OrderItems).Returns(orderItemRepo.Object);

        var result = await _assignedItemsHandler.Handle(
            new GetMyAssignedOrderItemsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetMyAssignedOrderItems_Should_Fail_When_No_Printer_Profile()
    {
        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<PrinterProfile>().AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var result = await _assignedItemsHandler.Handle(
            new GetMyAssignedOrderItemsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.Messages, Contains.Item("No printer profile found for this user. Create one first."));
    }

    #endregion
}
