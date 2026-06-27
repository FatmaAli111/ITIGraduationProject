using ITIGraduationProject.Application.Features.Orders.Commands.Mapping;
using ITIGraduationProject.Application.Features.Orders.Queries.Handlers;
using ITIGraduationProject.Application.Features.Orders.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Orders;

[TestFixture]
public class OrdersQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private GetUserOrdersQueryHandler _handler = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        new OrderMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _handler = new GetUserOrdersQueryHandler(_uow.Object);
    }

    [Test]
    public async Task GetUserOrders_Should_Return_Data()
    {
        var userId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = "WLY-2026-12345",
                SubTotal = 100m,
                TotalAmount = 108m,
                OrderStatus = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Shipment = new Shipment
                {
                    TrackingNumber = "TRK-001",
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5)
                },
                OrderItems = new List<OrderItem>
                {
                    new()
                    {
                        DesignId = Guid.NewGuid(),
                        Quantity = 1,
                        UnitPrice = 100m,
                        SnapshotImageURL = "snap.png",
                        PriceBreakdown = "Size: M",
                        Design = new Design
                        {
                            Product = new Product { Name = "T-Shirt" }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrderNumber = "WLY-2026-99999",
                SubTotal = 50m,
                TotalAmount = 54m,
                OrderStatus = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var repo = new Mock<IOrderRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(orders.AsQueryable().BuildMock());

        _uow.Setup(x => x.Orders).Returns(repo.Object);

        var result = await _handler.Handle(new GetUserOrdersQuery(userId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].OrderNumber, Is.EqualTo("WLY-2026-12345"));
        Assert.That(result.Data[0].OrderItems, Has.Count.EqualTo(1));
        Assert.That(result.Data[0].OrderItems[0].DesignName, Is.EqualTo("T-Shirt"));
    }

    [Test]
    public async Task GetUserOrders_Should_Return_Empty_Result()
    {
        var userId = Guid.NewGuid();

        var repo = new Mock<IOrderRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Order>().AsQueryable().BuildMock());

        _uow.Setup(x => x.Orders).Returns(repo.Object);

        var result = await _handler.Handle(new GetUserOrdersQuery(userId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.Meta, Is.EqualTo("No orders found for this user."));
    }
}
