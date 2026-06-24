using ITIGraduationProject.Application.Features.Shop.Queries.Handlers;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Shop.Queries
{
    [TestFixture]
    public class GetProductImagesQueryHandlerTests
    {
        [Test]
        public async Task Handle_ReturnsImagesForRequestedProduct()
        {
            var productId = Guid.NewGuid();
            var images = new List<ProductImage>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ImageUrl = "/uploads/products/front.jpg",
                    ViewAngle = ViewAngle.Front,
                    IsPrimary = true,
                    DisplayOrder = 1
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ImageUrl = "/uploads/products/back.jpg",
                    ViewAngle = ViewAngle.Back,
                    IsPrimary = false,
                    DisplayOrder = 2
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ImageUrl = "/uploads/products/other.jpg",
                    ViewAngle = ViewAngle.Left,
                    IsPrimary = false,
                    DisplayOrder = 3
                }
            };

            var handler = new GetProductImagesQueryHandler(new FakeUnitOfWork(images));
            var response = await handler.Handle(new GetProductImagesQuery(productId), CancellationToken.None);

            Assert.That(response.Succeeded, Is.True);
            Assert.That(response.Data, Has.Count.EqualTo(2));
            Assert.That(response.Data.Select(x => x.ImageUrl), Is.EquivalentTo(new[] { "/uploads/products/front.jpg", "/uploads/products/back.jpg" }));
        }

        private sealed class FakeUnitOfWork : IUnitOfWork
        {
            public FakeUnitOfWork(IEnumerable<ProductImage> images)
            {
                ProductImages = new FakeProductImageRepository(images);
            }

            public IProductRepository Products => throw new NotImplementedException();
            public IDesignRepository Designs => throw new NotImplementedException();
            public IOrderRepository Orders => throw new NotImplementedException();
            public IUserRepository Users => throw new NotImplementedException();
            public ITemplateRepository Templates => throw new NotImplementedException();
            public ICouponRepository Coupons => throw new NotImplementedException();
            public IModerationReportRepository ModerationReports => throw new NotImplementedException();
            public IAiChatSessionRepository AiChatSessions => throw new NotImplementedException();
            public IShipmentRepository Shipments => throw new NotImplementedException();
            public IRewardRepository Rewards => throw new NotImplementedException();
            public IGraphicAssetRepository GraphicAssets => throw new NotImplementedException();
            public INotificationRepository Notifications => throw new NotImplementedException();
            public IRefreshTokenRepository RefreshTokens => throw new NotImplementedException();
            public ICategoryRepository Categories => throw new NotImplementedException();
            public ICommunityInteractionRepository CommunityInteractions => throw new NotImplementedException();
            public IPrinterProfileRepository PrinterProfiles => throw new NotImplementedException();
            public IProductImageRepository ProductImages { get; }

            public IAiChatMessageRepository AiChatMessages => throw new NotImplementedException();

            public Task<int> SaveChangesAsync() => Task.FromResult(0);
        }

        private sealed class FakeProductImageRepository : IProductImageRepository
        {
            private readonly IQueryable<ProductImage> _images;

            public FakeProductImageRepository(IEnumerable<ProductImage> images)
            {
                _images = images.AsQueryable();
            }

            public Task<ProductImage?> GetByIdAsync(Guid id) => Task.FromResult(_images.FirstOrDefault(x => x.Id == id));

            public IQueryable<ProductImage> GetTableNoTracking() => _images;

            public IQueryable<ProductImage> GetTableAsTracking() => _images;

            public Task<ProductImage> AddAsync(ProductImage entity) => throw new NotImplementedException();

            public Task AddRangeAsync(ICollection<ProductImage> entities) => throw new NotImplementedException();

            public void Update(ProductImage entity) => throw new NotImplementedException();

            public void UpdateRange(ICollection<ProductImage> entities) => throw new NotImplementedException();

            public void Delete(ProductImage entity) => throw new NotImplementedException();

            public void DeleteRange(ICollection<ProductImage> entities) => throw new NotImplementedException();
        }
    }
}
