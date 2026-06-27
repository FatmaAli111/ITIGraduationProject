using ITIGraduationProject.Application.CQRS.Handlers.Queries;
using ITIGraduationProject.Application.CQRS.Queries;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.StudioDesign;

[TestFixture]
public class StudioDesignQueryHandlersTests
{
    private Mock<IDesignRepository> _designRepo = null!;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _designRepo = new Mock<IDesignRepository>();
        _userId = Guid.NewGuid();
    }

    [Test]
    public async Task GetDesignById_Should_Return_Design_When_Found()
    {
        var designId = Guid.NewGuid();
        var design = new Design
        {
            Id = designId,
            UserId = _userId,
            ProductId = Guid.NewGuid(),
            CanvasStateJSON = "{\"layers\":[]}",
            SnapshotImageURL = "/snap.png",
            Status = DesignStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _designRepo.Setup(r => r.GetByIdAsync(designId)).ReturnsAsync(design);

        var handler = new GetDesignByIdQueryHandler(_designRepo.Object);
        var result = await handler.Handle(new GetDesignByIdQuery { DesignId = designId }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Id, Is.EqualTo(designId));
        Assert.That(result.Data.UserId, Is.EqualTo(_userId));
        Assert.That(result.Data.CanvasStateJSON, Is.EqualTo("{\"layers\":[]}"));
    }

    [Test]
    public async Task GetDesignById_Should_Return_NotFound_When_Missing()
    {
        _designRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Design?)null);

        var handler = new GetDesignByIdQueryHandler(_designRepo.Object);
        var result = await handler.Handle(new GetDesignByIdQuery { DesignId = Guid.NewGuid() }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Design not found"));
    }

    [Test]
    public async Task GetUserDesigns_Should_Return_Designs_When_Found()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Hoodie" };
        var designs = new List<Design>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Product = product,
                ProductId = product.Id,
                SnapshotImageURL = "/snap.png",
                Status = DesignStatus.Draft,
                CalculatedPrice = 50m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _designRepo.Setup(r => r.GetByUserAsync(_userId)).ReturnsAsync(designs);

        var handler = new GetUserDesignsQueryHandler(_designRepo.Object);
        var result = await handler.Handle(new GetUserDesignsQuery { UserId = _userId }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data![0].ProductName, Is.EqualTo("Hoodie"));
    }

    [Test]
    public async Task GetUserDesigns_Should_Return_Empty_When_None()
    {
        _designRepo.Setup(r => r.GetByUserAsync(_userId)).ReturnsAsync(new List<Design>());

        var handler = new GetUserDesignsQueryHandler(_designRepo.Object);
        var result = await handler.Handle(new GetUserDesignsQuery { UserId = _userId }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Is.Empty);
        Assert.That(result.Message, Is.EqualTo("No designs found"));
    }
}
