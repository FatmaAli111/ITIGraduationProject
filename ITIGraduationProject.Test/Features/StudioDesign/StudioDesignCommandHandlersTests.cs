using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.CQRS.Handlers.Commands;
using ITIGraduationProject.Application.DTOs.Design;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.StudioDesign;

[TestFixture]
public class StudioDesignCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IDesignRepository> _designRepo = null!;
    private Mock<IProductRepository> _productRepo = null!;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _designRepo = new Mock<IDesignRepository>();
        _productRepo = new Mock<IProductRepository>();
        _userId = Guid.NewGuid();
    }

    [Test]
    public async Task SaveDesignDraft_Should_Succeed_When_Owner()
    {
        var designId = Guid.NewGuid();
        var design = new Design
        {
            Id = designId,
            UserId = _userId,
            ProductId = Guid.NewGuid(),
            CanvasStateJSON = "{}",
            Status = DesignStatus.Private
        };

        _designRepo.Setup(r => r.GetByIdAsync(designId)).ReturnsAsync(design);
        _designRepo.Setup(r => r.Update(It.IsAny<Design>()));
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new SaveDesignDraftCommandHandler(_uow.Object, _designRepo.Object);
        var result = await handler.Handle(new SaveDesignDraftCommand
        {
            UserId = _userId,
            DesignId = designId
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.True);
        Assert.That(design.Status, Is.EqualTo(DesignStatus.Draft));
        _designRepo.Verify(r => r.Update(design), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveDesignDraft_Should_Return_NotFound_When_Design_Missing()
    {
        _designRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Design?)null);

        var handler = new SaveDesignDraftCommandHandler(_uow.Object, _designRepo.Object);
        var result = await handler.Handle(new SaveDesignDraftCommand
        {
            UserId = _userId,
            DesignId = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Design not found"));
    }

    [Test]
    public async Task SaveDesignDraft_Should_Return_Unauthorized_When_Not_Owner()
    {
        var designId = Guid.NewGuid();
        var design = new Design
        {
            Id = designId,
            UserId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            CanvasStateJSON = "{}",
            Status = DesignStatus.Private
        };

        _designRepo.Setup(r => r.GetByIdAsync(designId)).ReturnsAsync(design);

        var handler = new SaveDesignDraftCommandHandler(_uow.Object, _designRepo.Object);
        var result = await handler.Handle(new SaveDesignDraftCommand
        {
            UserId = _userId,
            DesignId = designId
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Unauthorized"));
    }

    [Test]
    public async Task CalculatePrice_Should_Return_Total_When_Product_Exists()
    {
        var productId = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product
        {
            Id = productId,
            Name = "T-Shirt",
            BasePrice = 100m
        });

        var handler = new CalculatePriceCommandHandler(_productRepo.Object);
        var result = await handler.Handle(new CalculatePriceCommand
        {
            PriceCalculation = new DesignPriceCalculationDTO
            {
                ProductId = productId,
                SelectedFabric = FabricType.Cotton,
                SelectedPrintMethod = PrintMethodType.DirectToGarment,
                SelectedSize = ProductSize.L
            }
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.BasePrice, Is.EqualTo(100m));
        Assert.That(result.Data.FabricSurcharge, Is.EqualTo(5m));
        Assert.That(result.Data.PrintMethodSurcharge, Is.EqualTo(12m));
        Assert.That(result.Data.SizeSurcharge, Is.EqualTo(2m));
        Assert.That(result.Data.TotalPrice, Is.EqualTo(119m));
    }

    [Test]
    public async Task CalculatePrice_Should_Return_NotFound_When_Product_Missing()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var handler = new CalculatePriceCommandHandler(_productRepo.Object);
        var result = await handler.Handle(new CalculatePriceCommand
        {
            PriceCalculation = new DesignPriceCalculationDTO
            {
                ProductId = Guid.NewGuid()
            }
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Product not found"));
    }
}
