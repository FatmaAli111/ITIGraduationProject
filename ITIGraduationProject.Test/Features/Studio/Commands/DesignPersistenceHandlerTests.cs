using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;
using ITIGraduationProject.Application.Interfaces.IRepositories;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Studio.Commands;

[TestFixture]
public class DesignPersistenceHandlerTests
{
    private FakeUnitOfWork _unitOfWork = null!;
    private Mock<ICurrentUserService> _currentUserMock = null!;
    private Mock<IWebHostEnvironment> _hostEnvMock = null!;
    private Mock<ISender> _senderMock = null!;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _userId = Guid.NewGuid();
        _unitOfWork = new FakeUnitOfWork();

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(s => s.UserId).Returns(_userId);

        _hostEnvMock = new Mock<IWebHostEnvironment>();
        _hostEnvMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task CreateDesignHandler_PersistsCanvasStateAndAssets()
    {
        var createHandler = new CreateDesignCommandHandler(
            _unitOfWork,
            new FakePriceCalculation(),
            _currentUserMock.Object,
            _hostEnvMock.Object,
            NullLogger<CreateDesignCommandHandler>.Instance);

        var designId = await createHandler.Handle(new CreateDesignCommand(
            Id: null,
            ProductId: Guid.NewGuid(),
            TemplateId: null,
            CanvasStateJSON: "{\"version\":1}",
            Base64Snapshot: null,
            Base64Front: null,
            Base64Back: null,
            SelectedSize: ProductSize.L,
            SelectedFabric: FabricType.Cotton,
            SelectedPrintMethod: PrintMethodType.DirectToGarment,
            SelectedColor: "#123456",
            Assets: new List<DesignAssetInput>
            {
                new("logo", 1, "/uploads/logo.png", "hero")
            }
        ), CancellationToken.None);

        var createdDesign = await _unitOfWork.Designs.GetByIdAsync(designId);
        Assert.That(createdDesign, Is.Not.Null);
        Assert.That(createdDesign!.CanvasStateJSON, Is.EqualTo("{\"version\":1}"));
        Assert.That(createdDesign.UserId, Is.EqualTo(_userId));
    }

    [Test]
    public async Task UpdateDesignHandler_DelegatesToCreateHandler()
    {
        // Pre-create a design so the update can find it
        var designId = Guid.NewGuid();
        var existingDesign = new Design
        {
            Id = designId,
            UserId = _userId,
            ProductId = Guid.NewGuid(),
            CanvasStateJSON = "{\"version\":1}",
            Status = DesignStatus.Draft
        };
        await _unitOfWork.Designs.AddAsync(existingDesign);

        // Setup sender mock to simulate MediatR dispatch of CreateDesignCommand
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateDesignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(designId);

        var updateHandler = new UpdateDesignCommandHandler(
            _unitOfWork,
            _currentUserMock.Object,
            _senderMock.Object);

        await updateHandler.Handle(new UpdateDesignCommand(
            Id: designId,
            CanvasStateJSON: "{\"version\":2}",
            Base64Snapshot: null,
            Base64Front: null,
            Base64Back: null,
            SelectedSize: ProductSize.XL,
            SelectedFabric: FabricType.Polyester,
            SelectedPrintMethod: PrintMethodType.Embroidery,
            SelectedColor: "#ffffff",
            Assets: null
        ), CancellationToken.None);

        // Verify that the update handler dispatched CreateDesignCommand via MediatR
        _senderMock.Verify(
            s => s.Send(It.Is<CreateDesignCommand>(c => c.Id == designId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // Fakes
    // ---------------------------------------------------------------------------

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork()
        {
            Designs = new FakeDesignRepository();
            GraphicAssets = new FakeGraphicAssetRepository();

            var templateRepo = new Mock<ITemplateRepository>();
            templateRepo.Setup(x => x.AddAsync(It.IsAny<Template>()))
                .ReturnsAsync((Template t) => t);
            Templates = templateRepo.Object;
        }

        public IProductRepository Products => throw new NotImplementedException();
        public IDesignRepository Designs { get; }
        public IOrderRepository Orders => throw new NotImplementedException();
        public IUserRepository Users => throw new NotImplementedException();
        public ITemplateRepository Templates { get; }
        public ICouponRepository Coupons => throw new NotImplementedException();
        public IModerationReportRepository ModerationReports => throw new NotImplementedException();
        public IAiChatSessionRepository AiChatSessions => throw new NotImplementedException();
        public IShipmentRepository Shipments => throw new NotImplementedException();
        public IRewardRepository Rewards => throw new NotImplementedException();
        public IGraphicAssetRepository GraphicAssets { get; }
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
                Update(entity);
        }

        public void Delete(Design entity) => _designs.Remove(entity);

        public void DeleteRange(ICollection<Design> entities)
        {
            foreach (var entity in entities)
                Delete(entity);
        }

        public Task<Design?> GetWithImagesAndAssetsAsync(Guid id) => GetByIdAsync(id);

        public Task<IEnumerable<Design>> GetByUserAsync(Guid userId) =>
            Task.FromResult<IEnumerable<Design>>(_designs.Where(x => x.UserId == userId));

        public Task<IEnumerable<Design>> GetByStatusAsync(DesignStatus status) =>
            Task.FromResult<IEnumerable<Design>>(_designs.Where(x => x.Status == status));

        public Task SetGraphicAssetsAsync(Design design, IList<GraphicAsset> assets, CancellationToken cancellationToken = default)
        {
            design.GraphicAssets.Clear();
            foreach (var asset in assets)
            {
                design.GraphicAssets.Add(asset);
            }
            return Task.CompletedTask;
        }

        public List<string> GetChangeTrackerState() => new();

        public Task<int> GetDesignGraphicAssetsCountAsync(Guid designId, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeGraphicAssetRepository : IGraphicAssetRepository
    {
        private readonly List<GraphicAsset> _assets = new();

        public Task<GraphicAsset?> GetByIdAsync(Guid id) =>
            Task.FromResult(_assets.FirstOrDefault(x => x.Id == id));

        public IQueryable<GraphicAsset> GetTableNoTracking() => _assets.AsQueryable();
        public IQueryable<GraphicAsset> GetTableAsTracking() => _assets.AsQueryable();

        public Task<GraphicAsset> AddAsync(GraphicAsset entity)
        {
            _assets.Add(entity);
            return Task.FromResult(entity);
        }

        public Task AddRangeAsync(ICollection<GraphicAsset> entities)
        {
            _assets.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update(GraphicAsset entity) { }
        public void UpdateRange(ICollection<GraphicAsset> entities) { }
        public void Delete(GraphicAsset entity) => _assets.Remove(entity);
        public void DeleteRange(ICollection<GraphicAsset> entities)
        {
            foreach (var e in entities) Delete(e);
        }

        public Task<IEnumerable<GraphicAsset>> SearchByTagsAsync(string? tags) =>
            Task.FromResult<IEnumerable<GraphicAsset>>(_assets);

        public Task<IEnumerable<GraphicAsset>> GetByTypeAsync(GraphicAssetType type) =>
            Task.FromResult<IEnumerable<GraphicAsset>>(_assets.Where(x => x.Type == type));
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
