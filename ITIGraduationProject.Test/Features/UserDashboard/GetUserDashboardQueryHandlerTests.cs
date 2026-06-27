using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Application.Features.UserDashboard.Queries.Handlers;
using ITIGraduationProject.Application.Features.UserDashboard.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Test.Features.UserDashboard
{
    [TestFixture]
    public class GetUserDashboardQueryHandlerTests
    {
        private Mock<IUnitOfWork> _uow = null!;
        private GetUserDashboardQueryHandler _handler = null!;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _uow = new Mock<IUnitOfWork>();
            _handler = new GetUserDashboardQueryHandler(_uow.Object);
            _userId = Guid.NewGuid();
        }

        [Test]
        public async Task Handle_Should_Return_Empty_Safe_Dashboard_For_User_With_No_Data()
        {
            SetupUser(new User { Id = _userId, Name = "Ahmed Hassan", IsDeleted = false });
            SetupDesigns(Array.Empty<Design>());
            SetupOrders(Array.Empty<Order>());
            SetupTemplates(Array.Empty<Template>());
            SetupCommunityInteractions(Array.Empty<CommunityInteraction>());
            SetupProducts(Array.Empty<Product>());

            var result = await _handler.Handle(new GetUserDashboardQuery(_userId), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(result.Data.GreetingName, Is.EqualTo("Ahmed"));
                Assert.That(result.Data.Stats.SavedDesigns, Is.EqualTo(0));
                Assert.That(result.Data.Stats.ActiveOrders, Is.EqualTo(0));
                Assert.That(result.Data.Stats.CommunityLikes, Is.EqualTo(0));
                Assert.That(result.Data.FeaturedDraft, Is.Null);
                Assert.That(result.Data.ActiveOrder, Is.Null);
                Assert.That(result.Data.Recommendations, Is.Empty);
                Assert.That(result.Data.RecentActivity, Is.Empty);
                Assert.That(result.Data.DesignChecklist, Has.Count.EqualTo(4));
                Assert.That(result.Data.DesignChecklist.All(c => !c.Complete), Is.True);
            });
        }

        [Test]
        public async Task Handle_Should_Return_Latest_Draft_As_FeaturedDraft()
        {
            var product = new Product { Id = Guid.NewGuid(), Name = "Heavyweight Hoodie" };
            var olderDraft = CreateDesign(
                DesignStatus.Draft,
                product,
                createdAt: DateTime.UtcNow.AddDays(-3),
                updatedAt: DateTime.UtcNow.AddDays(-2),
                titleTemplate: "Older Draft");
            var latestDraft = CreateDesign(
                DesignStatus.Draft,
                product,
                createdAt: DateTime.UtcNow.AddDays(-1),
                updatedAt: DateTime.UtcNow.AddHours(-2),
                titleTemplate: "Prism Field Remix");
            var published = CreateDesign(
                DesignStatus.Public,
                product,
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow,
                titleTemplate: "Published Design");

            SetupUser(new User { Id = _userId, Name = "Ahmed", IsDeleted = false });
            SetupDesigns(new[] { olderDraft, latestDraft, published });
            SetupOrders(Array.Empty<Order>());
            SetupTemplates(Array.Empty<Template>());
            SetupCommunityInteractions(Array.Empty<CommunityInteraction>());
            SetupProducts(Array.Empty<Product>());

            var result = await _handler.Handle(new GetUserDashboardQuery(_userId), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data.FeaturedDraft, Is.Not.Null);
                Assert.That(result.Data.FeaturedDraft!.Id, Is.EqualTo(latestDraft.Id));
                Assert.That(result.Data.FeaturedDraft.Title, Is.EqualTo("Prism Field Remix"));
                Assert.That(result.Data.FeaturedDraft.Product, Is.EqualTo("Heavyweight Hoodie"));
            });
        }

        [Test]
        public async Task Handle_Should_Return_Latest_Active_Order()
        {
            var olderActive = new Order
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                OrderNumber = "WLY-2026-00471",
                OrderStatus = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false,
                Shipment = new Shipment { EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3) }
            };
            var latestActive = new Order
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                OrderNumber = "WLY-2026-00498",
                OrderStatus = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                Shipment = new Shipment { EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7) }
            };
            var delivered = new Order
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                OrderNumber = "WLY-2026-00400",
                OrderStatus = OrderStatus.Delivered,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            SetupUser(new User { Id = _userId, Name = "Ahmed", IsDeleted = false });
            SetupDesigns(Array.Empty<Design>());
            SetupOrders(new[] { olderActive, latestActive, delivered });
            SetupTemplates(Array.Empty<Template>());
            SetupCommunityInteractions(Array.Empty<CommunityInteraction>());
            SetupProducts(Array.Empty<Product>());

            var result = await _handler.Handle(new GetUserDashboardQuery(_userId), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data.ActiveOrder, Is.Not.Null);
                Assert.That(result.Data.ActiveOrder!.DisplayCode, Is.EqualTo("WLY-2026-00498"));
                Assert.That(result.Data.ActiveOrder.Status, Is.EqualTo("PROCESSING"));
                Assert.That(result.Data.Stats.ActiveOrders, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task Handle_Should_Limit_Recommendations_To_Four()
        {
            var category = new Category { Name = "Hoodies" };
            var products = Enumerable.Range(1, 6).Select(i => new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                BasePrice = 100 + i,
                PreviewImageURL = $"/uploads/products/p{i}.png",
                AverageRating = 4.5m,
                ReviewCount = i * 100,
                IsAvailable = true,
                IsDeleted = false,
                Category = category,
                AvailableColors = ProductAvailableColors.Black | ProductAvailableColors.White
            }).ToList();

            SetupUser(new User { Id = _userId, Name = "Ahmed", IsDeleted = false });
            SetupDesigns(Array.Empty<Design>());
            SetupOrders(Array.Empty<Order>());
            SetupTemplates(Array.Empty<Template>());
            SetupCommunityInteractions(Array.Empty<CommunityInteraction>());
            SetupProducts(products);

            var result = await _handler.Handle(new GetUserDashboardQuery(_userId), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Recommendations, Has.Count.EqualTo(4));
                Assert.That(result.Data.Recommendations[0].Name, Is.EqualTo("Product 6"));
            });
        }

        [Test]
        public async Task Handle_Should_Return_Correct_Stats_Counts()
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var product = new Product { Id = Guid.NewGuid(), Name = "Tee" };

            var designs = new[]
            {
                CreateDesign(DesignStatus.Draft, product, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1)),
                CreateDesign(DesignStatus.Private, product, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-2)),
                CreateDesign(DesignStatus.Public, product, weekAgo.AddDays(-1), weekAgo.AddDays(-1))
            };

            var orders = new[]
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = _userId,
                    OrderStatus = OrderStatus.Pending,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = _userId,
                    OrderStatus = OrderStatus.Shipped,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = _userId,
                    OrderStatus = OrderStatus.Delivered,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var templates = new[]
            {
                new Template
                {
                    Id = Guid.NewGuid(),
                    CreatorUserId = _userId,
                    LikesCount = 80,
                    IsDeleted = false
                },
                new Template
                {
                    Id = Guid.NewGuid(),
                    CreatorUserId = _userId,
                    LikesCount = 48,
                    IsDeleted = false
                }
            };

            var interactions = new[]
            {
                new CommunityInteraction
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templates[0].Id,
                    InteractionType = InteractionType.Like,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    IsDeleted = false
                },
                new CommunityInteraction
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templates[1].Id,
                    InteractionType = InteractionType.Like,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsDeleted = false
                }
            };

            SetupUser(new User { Id = _userId, Name = "Ahmed", IsDeleted = false });
            SetupDesigns(designs);
            SetupOrders(orders);
            SetupTemplates(templates);
            SetupCommunityInteractions(interactions);
            SetupProducts(Array.Empty<Product>());

            var result = await _handler.Handle(new GetUserDashboardQuery(_userId), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Stats.SavedDesigns, Is.EqualTo(3));
                Assert.That(result.Data.Stats.SavedDesignsDelta, Is.EqualTo("+2 this week"));
                Assert.That(result.Data.Stats.ActiveOrders, Is.EqualTo(2));
                Assert.That(result.Data.Stats.ActiveOrdersDelta, Is.EqualTo("1 needs review"));
                Assert.That(result.Data.Stats.CommunityLikes, Is.EqualTo(128));
                Assert.That(result.Data.Stats.CommunityLikesDelta, Is.EqualTo("+1 this week"));
            });
        }

        private static Design CreateDesign(
            DesignStatus status,
            Product product,
            DateTime createdAt,
            DateTime? updatedAt,
            string titleTemplate = "Template Name")
        {
            return new Design
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                SnapshotImageURL = "/uploads/designs/example.png",
                CanvasStateJSON = "{}",
                Template = new Template { Name = titleTemplate },
                GraphicAssets = new HashSet<GraphicAsset>(),
                DesignImages = new HashSet<DesignImage>(),
                CartItems = new HashSet<CartItem>(),
                OrderItems = new HashSet<OrderItem>()
            };
        }

        private void SetupUser(User user)
        {
            var repo = new Mock<IUserRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(new List<User> { user }.AsQueryable().BuildMock());
            _uow.Setup(x => x.Users).Returns(repo.Object);
        }

        private void SetupDesigns(IEnumerable<Design> designs)
        {
            foreach (var design in designs)
                design.UserId = _userId;

            var repo = new Mock<IDesignRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(designs.AsQueryable().BuildMock());
            _uow.Setup(x => x.Designs).Returns(repo.Object);
        }

        private void SetupOrders(IEnumerable<Order> orders)
        {
            var repo = new Mock<IOrderRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(orders.AsQueryable().BuildMock());
            _uow.Setup(x => x.Orders).Returns(repo.Object);
        }

        private void SetupTemplates(IEnumerable<Template> templates)
        {
            var repo = new Mock<ITemplateRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(templates.AsQueryable().BuildMock());
            _uow.Setup(x => x.Templates).Returns(repo.Object);
        }

        private void SetupCommunityInteractions(IEnumerable<CommunityInteraction> interactions)
        {
            var repo = new Mock<ICommunityInteractionRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(interactions.AsQueryable().BuildMock());
            _uow.Setup(x => x.CommunityInteractions).Returns(repo.Object);
        }

        private void SetupProducts(IEnumerable<Product> products)
        {
            var repo = new Mock<IProductRepository>();
            repo.Setup(x => x.GetTableNoTracking())
                .Returns(products.AsQueryable().BuildMock());
            _uow.Setup(x => x.Products).Returns(repo.Object);
        }
    }
}
