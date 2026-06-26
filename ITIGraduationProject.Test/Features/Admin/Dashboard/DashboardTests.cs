using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Handlers;
using ITIGraduationProject.Application.Features.Admin.Dashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Moq;
using MockQueryable.Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Test.Features.Admin.Dashboard
{
   public class DashboardTests
    {

        private Mock<IUnitOfWork> _uow = null!;
        private GetDashboardOverviewQueryHandler _overviewHandler = null!;

        [SetUp]
        public void Setup()
        {
            _uow = new Mock<IUnitOfWork>();

            _overviewHandler = new GetDashboardOverviewQueryHandler(_uow.Object);
        }

        [Test]
        public async Task GetDashboardOverview_Should_Return_Correct_Data()
        {
            var users = new List<User>
    {
        new() { Id = Guid.NewGuid(), IsDeleted = false },
        new() { Id = Guid.NewGuid(), IsDeleted = false },
        new() { Id = Guid.NewGuid(), IsDeleted = true }
    };

            var orders = new List<Order>
    {
        new()
        {
            Id = Guid.NewGuid(),
            IsDeleted = false,
            TotalAmount = 100,
            OrderStatus = OrderStatus.Pending
        },
        new()
        {
            Id = Guid.NewGuid(),
            IsDeleted = false,
            TotalAmount = 200,
            OrderStatus = OrderStatus.Delivered
        },
        new()
        {
            Id = Guid.NewGuid(),
            IsDeleted = false,
            TotalAmount = 300,
            OrderStatus = OrderStatus.Cancelled
        }
    };

            var products = new List<Product>
    {
        new(){Id=Guid.NewGuid(),IsDeleted=false},
        new(){Id=Guid.NewGuid(),IsDeleted=false}
    };

            var templates = new List<Template>
    {
        new(){Id=Guid.NewGuid(),IsDeleted=false,IsPublic=true},
        new(){Id=Guid.NewGuid(),IsDeleted=false,IsPublic=false}
    };

            var reports = new List<ModerationReport>
    {
        new()
        {
            Id=Guid.NewGuid(),
            IsDeleted=false,
            Status=ModerationReportStatus.Pending
        },
        new()
        {
            Id=Guid.NewGuid(),
            IsDeleted=false,
            Status=ModerationReportStatus.Reviewed
        }
    };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(x => x.GetTableNoTracking())
                .Returns(users.AsQueryable().BuildMock());

            var orderRepo = new Mock<IOrderRepository>();
            orderRepo.Setup(x => x.GetTableNoTracking())
                .Returns(orders.AsQueryable().BuildMock());

            var productRepo = new Mock<IProductRepository>();
            productRepo.Setup(x => x.GetTableNoTracking())
                .Returns(products.AsQueryable().BuildMock());

            var templateRepo = new Mock<ITemplateRepository>();
            templateRepo.Setup(x => x.GetTableNoTracking())
                .Returns(templates.AsQueryable().BuildMock());

            var reportRepo = new Mock<IModerationReportRepository>();
            reportRepo.Setup(x => x.GetTableNoTracking())
                .Returns(reports.AsQueryable().BuildMock());

            _uow.Setup(x => x.Users).Returns(userRepo.Object);
            _uow.Setup(x => x.Orders).Returns(orderRepo.Object);
            _uow.Setup(x => x.Products).Returns(productRepo.Object);
            _uow.Setup(x => x.Templates).Returns(templateRepo.Object);
            _uow.Setup(x => x.ModerationReports).Returns(reportRepo.Object);

            var result = await _overviewHandler.Handle(
                new GetDashboardOverviewQuery(),
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(result.Data.TotalUsers, Is.EqualTo(2));
                Assert.That(result.Data.TotalOrders, Is.EqualTo(3));
                Assert.That(result.Data.PendingOrders, Is.EqualTo(1));
                Assert.That(result.Data.TotalRevenue, Is.EqualTo(300));
                Assert.That(result.Data.TotalProducts, Is.EqualTo(2));
                Assert.That(result.Data.TotalTemplates, Is.EqualTo(2));
                Assert.That(result.Data.PublicTemplates, Is.EqualTo(1));
                Assert.That(result.Data.PendingModerationReports, Is.EqualTo(1));
            });
        }
        [Test]
        public async Task GetOrdersByStatus_Should_Return_Correct_Counts()
        {
            var orders = new List<Order>
    {
        new()
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Pending,
            IsDeleted = false
        },
        new()
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Pending,
            IsDeleted = false
        },
        new()
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Delivered,
            IsDeleted = false
        },
        new()
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Cancelled,
            IsDeleted = true
        }
    };

            var repo = new Mock<IOrderRepository>();

            repo.Setup(x => x.GetTableNoTracking())
                .Returns(orders.AsQueryable().BuildMock());

            _uow.Setup(x => x.Orders)
                .Returns(repo.Object);

            var handler = new GetOrdersByStatusQueryHandler(_uow.Object);

            var result = await handler.Handle(
                new GetOrdersByStatusQuery(),
                CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.Data.First(x => x.Status == OrderStatus.Pending.ToString()).Count,
                Is.EqualTo(2));

            Assert.That(
                result.Data.First(x => x.Status == OrderStatus.Delivered.ToString()).Count,
                Is.EqualTo(1));

            Assert.That(
                result.Data.First(x => x.Status == OrderStatus.Cancelled.ToString()).Count,
                Is.EqualTo(0));
        }

        [Test]
        public async Task GetRecentOrders_Should_Return_Latest_Orders()
        {
            var orders = new List<Order>
    {
        new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            TotalAmount = 100,
            OrderStatus = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsDeleted = false,
            User = new User { Name = "Fatma" }
        },
        new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2",
            TotalAmount = 200,
            OrderStatus = OrderStatus.Delivered,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            User = new User { Name = "Ali" }
        },
        new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-3",
            TotalAmount = 300,
            OrderStatus = OrderStatus.Cancelled,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false,
            User = new User { Name = "Sara" }
        }
    };

            var repo = new Mock<IOrderRepository>();

            repo.Setup(x => x.GetTableNoTracking())
                .Returns(orders.AsQueryable().BuildMock());

            _uow.Setup(x => x.Orders)
                .Returns(repo.Object);

            var handler = new GetRecentOrdersQueryHandler(_uow.Object);

            var result = await handler.Handle(
                new GetRecentOrdersQuery(2),
                CancellationToken.None);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.Count, Is.EqualTo(2));

            Assert.That(result.Data[0].OrderNumber, Is.EqualTo("ORD-2"));
            Assert.That(result.Data[1].OrderNumber, Is.EqualTo("ORD-3"));
        }
    }
}
