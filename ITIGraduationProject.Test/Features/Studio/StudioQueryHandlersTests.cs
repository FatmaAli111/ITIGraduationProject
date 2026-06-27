using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using ITIGraduationProject.Application.Features.Studio.Queries.GetAdminGraphicAssets;
using ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages;
using ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById;
using ITIGraduationProject.Application.Features.Studio.Queries.GetGraphicAssetById;
using ITIGraduationProject.Application.Features.Studio.Queries.GetGraphicAssets;
using ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization;
using ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts;
using ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions;
using ITIGraduationProject.Application.Features.Studio.Queries.GetUserDesigns;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MapsterMapper;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Studio;

[TestFixture]
public class StudioQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<Design, DesignResponseDto>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.SelectedSize, src => src.SelectedSize != null ? src.SelectedSize.ToString() : null)
            .Map(dest => dest.SelectedFabric, src => src.SelectedFabric != null ? src.SelectedFabric.ToString() : null)
            .Map(dest => dest.SelectedPrintMethod, src => src.SelectedPrintMethod != null ? src.SelectedPrintMethod.ToString() : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<AiChatSession, AiChatSessionDto>()
            .Map(dest => dest.SessionType, src => (int)src.SessionType);

        TypeAdapterConfig.GlobalSettings.NewConfig<AiChatMessage, AiChatMessageDto>()
            .Map(dest => dest.SessionId, src => src.AiChatSessionId);

        TypeAdapterConfig.GlobalSettings.NewConfig<Product, StudioProductListItemDto>()
            .Map(dest => dest.ThumbnailImageUrl, src => src.PreviewImageURL);

        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
    }

    [Test]
    public async Task GetDesignById_Should_Return_Design_When_Found()
    {
        var designId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "T-Shirt" };
        var designs = new List<Design>
        {
            new()
            {
                Id = designId,
                UserId = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                CanvasStateJSON = "{}",
                SnapshotImageURL = "/snap.png",
                Status = DesignStatus.Draft,
                CalculatedPrice = 99m
            }
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableNoTracking())
            .Returns(designs.AsQueryable().BuildMock());
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new GetDesignByIdQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetDesignByIdQuery(designId), CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(designId));
        Assert.That(result.ProductName, Is.EqualTo("T-Shirt"));
        Assert.That(result.Status, Is.EqualTo("Draft"));
    }

    [Test]
    public void GetDesignById_Should_Throw_When_Not_Found()
    {
        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Design>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new GetDesignByIdQueryHandler(_uow.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetDesignByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Test]
    public async Task GetUserDesigns_Should_Return_Designs_For_User()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Mug" };
        var designs = new List<Design>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = product.Id,
                Product = product,
                CanvasStateJSON = "{}",
                SnapshotImageURL = "/snap.png",
                Status = DesignStatus.Draft,
                CalculatedPrice = 25m
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                CanvasStateJSON = "{}",
                Status = DesignStatus.Draft
            }
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableNoTracking())
            .Returns(designs.AsQueryable().BuildMock());
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new GetUserDesignsQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetUserDesignsQuery(userId), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].ProductName, Is.EqualTo("Mug"));
    }

    [Test]
    public async Task GetUserDesigns_Should_Return_Empty_When_None()
    {
        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Design>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new GetUserDesignsQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetUserDesignsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetGraphicAssetById_Should_Return_Dto_When_Found()
    {
        var assetId = Guid.NewGuid();
        var assets = new List<GraphicAsset>
        {
            new()
            {
                Id = assetId,
                UserId = Guid.NewGuid(),
                Name = "Icon",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/icon.png",
                Tags = "fun"
            }
        };

        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableNoTracking())
            .Returns(assets.AsQueryable().BuildMock());
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);

        var handler = new GetGraphicAssetByIdQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetGraphicAssetByIdQuery(assetId), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(assetId));
        Assert.That(result.Name, Is.EqualTo("Icon"));
    }

    [Test]
    public async Task GetGraphicAssetById_Should_Return_Null_When_Not_Found()
    {
        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<GraphicAsset>().AsQueryable().BuildMock());
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);

        var handler = new GetGraphicAssetByIdQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetGraphicAssetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserAiChatSessions_Should_Return_Sessions()
    {
        var userId = Guid.NewGuid();
        var sessions = new List<AiChatSession>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionType = AiChatSessionType.DesignAssistance,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionType = AiChatSessionType.DesignAssistance,
                CreatedAt = DateTime.UtcNow
            }
        };

        var sessionRepo = new Mock<IAiChatSessionRepository>();
        sessionRepo.Setup(x => x.GetTableNoTracking())
            .Returns(sessions.AsQueryable().BuildMock());
        _uow.Setup(x => x.AiChatSessions).Returns(sessionRepo.Object);

        var handler = new GetUserAiChatSessionsQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetUserAiChatSessionsQuery(userId), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UserId, Is.EqualTo(userId));
    }

    [Test]
    public async Task GetAiChatMessages_Should_Return_Messages_For_Session()
    {
        var sessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        var sessions = new List<AiChatSession>
        {
            new()
            {
                Id = sessionId,
                UserId = Guid.NewGuid(),
                AiChatMessages = new List<AiChatMessage>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        AiChatSessionId = sessionId,
                        Sender = "user",
                        MessageText = "Hi",
                        SentAt = DateTime.UtcNow.AddMinutes(-1)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        AiChatSessionId = sessionId,
                        Sender = "ai",
                        MessageText = "Hello",
                        SentAt = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Id = otherSessionId,
                UserId = Guid.NewGuid(),
                AiChatMessages = new List<AiChatMessage>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        AiChatSessionId = otherSessionId,
                        Sender = "user",
                        MessageText = "Other",
                        SentAt = DateTime.UtcNow
                    }
                }
            }
        };

        var sessionRepo = new Mock<IAiChatSessionRepository>();
        sessionRepo.Setup(x => x.GetTableNoTracking())
            .Returns(sessions.AsQueryable().BuildMock());
        _uow.Setup(x => x.AiChatSessions).Returns(sessionRepo.Object);

        var handler = new GetAiChatMessagesQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetAiChatMessagesQuery(sessionId), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(m => m.SessionId == sessionId), Is.True);
    }

    [Test]
    public async Task GetStudioProducts_Should_Return_Products_With_Thumbnail()
    {
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Hoodie",
                BasePrice = 45m,
                PreviewImageURL = "/hoodie.png"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Blank",
                BasePrice = 10m,
                PreviewImageURL = string.Empty
            }
        };

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var handler = new GetStudioProductsQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetStudioProductsQuery(), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Hoodie"));
        Assert.That(result[0].ThumbnailImageUrl, Is.EqualTo("/hoodie.png"));
    }

    [Test]
    public async Task GetStudioProducts_Should_Return_Empty_When_None_Qualify()
    {
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "No Image",
                PreviewImageURL = string.Empty
            }
        };

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var handler = new GetStudioProductsQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetStudioProductsQuery(), CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetProductForCustomization_Should_Return_Mapped_Product()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Canvas Bag",
            BasePrice = 20m
        };

        var products = new List<Product> { product };
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(x => x.GetTableNoTracking())
            .Returns(products.AsQueryable().BuildMock());
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<StudioProductDetailDto>(product))
            .Returns(new StudioProductDetailDto
            {
                Id = productId,
                Name = "Canvas Bag",
                BasePrice = 20m
            });

        var handler = new GetProductForCustomizationQueryHandler(_uow.Object, mapper.Object);
        var result = await handler.Handle(new GetProductForCustomizationQuery(productId), CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(productId));
        Assert.That(result.Name, Is.EqualTo("Canvas Bag"));
        mapper.Verify(m => m.Map<StudioProductDetailDto>(product), Times.Once);
    }

    [Test]
    public void GetProductForCustomization_Should_Throw_When_Not_Found()
    {
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Product>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var mapper = new Mock<IMapper>();
        var handler = new GetProductForCustomizationQueryHandler(_uow.Object, mapper.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetProductForCustomizationQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Test]
    public async Task GetGraphicAssets_Should_Return_Filtered_Assets()
    {
        var currentUserId = Guid.NewGuid();
        var ownAssetId = Guid.NewGuid();
        var userAssetId = Guid.NewGuid();
        var adminAssetId = Guid.NewGuid();

        var assets = new List<GraphicAsset>
        {
            new()
            {
                Id = ownAssetId,
                UserId = currentUserId,
                Name = "Mine",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/mine.png"
            },
            new()
            {
                Id = userAssetId,
                UserId = Guid.NewGuid(),
                Name = "User Asset",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/user.png"
            },
            new()
            {
                Id = adminAssetId,
                UserId = Guid.NewGuid(),
                Name = "Admin Asset",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/admin.png"
            }
        };

        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableNoTracking())
            .Returns(assets.AsQueryable().BuildMock());
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);

        var adminService = new Mock<IAdminUserService>();
        adminService.Setup(s => s.GetUserRoleAsync(currentUserId)).ReturnsAsync("User");
        adminService.Setup(s => s.GetUserRoleAsync(assets[1].UserId)).ReturnsAsync("User");
        adminService.Setup(s => s.GetUserRoleAsync(assets[2].UserId)).ReturnsAsync("Admin");

        var handler = new GetGraphicAssetsQueryHandler(_uow.Object, adminService.Object);
        var result = await handler.Handle(
            new GetGraphicAssetsQuery(GraphicAssetType.Sticker, currentUserId),
            CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(a => a.Id), Does.Contain(ownAssetId));
        Assert.That(result.Select(a => a.Id), Does.Contain(userAssetId));
        Assert.That(result.Select(a => a.Id), Does.Not.Contain(adminAssetId));
    }

    [Test]
    public async Task GetAdminGraphicAssets_Should_Return_Only_Admin_Assets()
    {
        var adminUserId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();
        var adminAssetId = Guid.NewGuid();

        var assets = new List<GraphicAsset>
        {
            new()
            {
                Id = adminAssetId,
                UserId = adminUserId,
                Name = "Admin Sticker",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/admin.png"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = regularUserId,
                Name = "User Sticker",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/user.png"
            }
        };

        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableNoTracking())
            .Returns(assets.AsQueryable().BuildMock());
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);

        var adminService = new Mock<IAdminUserService>();
        adminService.Setup(s => s.GetUserRoleAsync(adminUserId)).ReturnsAsync("Admin");
        adminService.Setup(s => s.GetUserRoleAsync(regularUserId)).ReturnsAsync("User");

        var handler = new GetAdminGraphicAssetsQueryHandler(_uow.Object, adminService.Object);
        var result = await handler.Handle(
            new GetAdminGraphicAssetsQuery(GraphicAssetType.Sticker),
            CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(adminAssetId));
    }
}
