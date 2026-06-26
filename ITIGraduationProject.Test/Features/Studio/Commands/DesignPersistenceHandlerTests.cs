using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;
using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Studio.Commands;

[TestFixture]
public class DesignPersistenceHandlerTests
{
    [Test]
    public async Task CreateAndUpdateHandlers_PersistCanvasStateAndAssets()
    {
        var unitOfWork = new FakeUnitOfWork();
        var priceCalculation = new FakePriceCalculation();
        var createHandler = new CreateDesignCommandHandler(unitOfWork, priceCalculation);
        var updateHandler = new UpdateDesignCommandHandler(unitOfWork);

        var designId = await createHandler.Handle(new CreateDesignCommand(
            UserId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            TemplateId: null,
            CanvasStateJSON: "{\"version\":1}",
            SnapshotImageURL: "/uploads/design.png",
            SelectedSize: ProductSize.L,
            SelectedFabric: FabricType.Cotton,
            SelectedPrintMethod: PrintMethodType.DirectToGarment,
            SelectedColor: "#123456",
            Assets: new List<DesignAssetInput>
            {
                new("logo", 1, "/uploads/logo.png", "hero")
            }
        ), CancellationToken.None);

        var createdDesign = await unitOfWork.Designs.GetByIdAsync(designId);
        Assert.That(createdDesign, Is.Not.Null);
        Assert.That(createdDesign!.CanvasStateJSON, Is.EqualTo("{\"version\":1}"));
        Assert.That(createdDesign.GraphicAssets, Has.Count.EqualTo(1));
        Assert.That(createdDesign.GraphicAssets.Single().ImageUrl, Is.EqualTo("/uploads/logo.png"));

        await updateHandler.Handle(new UpdateDesignCommand(
            Id: designId,
            CanvasStateJSON: "{\"version\":2}",
            SnapshotImageURL: "/uploads/updated.png",
            SelectedSize: ProductSize.XL,
            SelectedFabric: FabricType.Polyester,
            SelectedPrintMethod: PrintMethodType.Embroidery,
            SelectedColor: "#ffffff",
            Assets: new List<DesignAssetInput>
            {
                new("updated-logo", 2, "/uploads/updated-logo.png", "updated")
            }
        ), CancellationToken.None);

        var updatedDesign = await unitOfWork.Designs.GetByIdAsync(designId);
        Assert.That(updatedDesign, Is.Not.Null);
        Assert.That(updatedDesign!.CanvasStateJSON, Is.EqualTo("{\"version\":2}"));
        Assert.That(updatedDesign.SnapshotImageURL, Is.EqualTo("/uploads/updated.png"));
        Assert.That(updatedDesign.GraphicAssets, Has.Count.EqualTo(1));
        Assert.That(updatedDesign.GraphicAssets.Single().ImageUrl, Is.EqualTo("/uploads/updated-logo.png"));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork()
        {
            Designs = new FakeDesignRepository();
        }

        public IProductRepository Products => throw new NotImplementedException();
        public IDesignRepository Designs { get; }
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
        public IProductImageRepository ProductImages => throw new NotImplementedException();
        public IPrinterProfileRepository PrinterProfiles => throw new NotImplementedException();
        public IAiChatMessageRepository AiChatMessages => throw new NotImplementedException();
        public IOrderItemRepository OrderItems => throw new NotImplementedException();


        public Task<int> SaveChangesAsync() => Task.FromResult(0);
    }

    private sealed class FakeDesignRepository : IDesignRepository
    {
        private readonly List<Design> _designs = new();

        public Task<Design?> GetByIdAsync(Guid id) => Task.FromResult(_designs.FirstOrDefault(x => x.Id == id));

        public IQueryable<Design> GetTableNoTracking() => _designs.AsQueryable();

        public IQueryable<Design> GetTableAsTracking() => _designs.AsQueryable();

        public Task<Design> AddAsync(Design entity)
        {
            _designs.Add(entity);
            return Task.FromResult(entity);
        }

        public Task AddRangeAsync(ICollection<Design> entities)
        {
            _designs.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update(Design entity)
        {
            var existing = _designs.FirstOrDefault(x => x.Id == entity.Id);
            if (existing is not null)
            {
                _designs.Remove(existing);
                _designs.Add(entity);
            }
        }

        public void UpdateRange(ICollection<Design> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public void Delete(Design entity) => _designs.Remove(entity);

        public void DeleteRange(ICollection<Design> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public Task<Design?> GetWithImagesAndAssetsAsync(Guid id) => GetByIdAsync(id);

        public Task<IEnumerable<Design>> GetByUserAsync(Guid userId) => Task.FromResult<IEnumerable<Design>>(_designs.Where(x => x.UserId == userId));

        public Task<IEnumerable<Design>> GetByStatusAsync(DesignStatus status) => Task.FromResult<IEnumerable<Design>>(_designs.Where(x => x.Status == status));
    }

    private sealed class FakePriceCalculation : IPriceCalculation
    {
        public Task<decimal> CalculatePriceAsync(
            Guid productId,
            FabricType? selectedFabric,
            PrintMethodType? selectedPrintMethod,
            ProductSize? selectedSize,
            CancellationToken cancellationToken = default) => Task.FromResult(100m);
    }
}
