using ITIGraduationProject.Application.Bases.Templates;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Mapping;
using ITIGraduationProject.Application.Features.Templates.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Templates;

[TestFixture]
public class TemplatesQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Guid _userId;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        new TemplateMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();
        _userId = Guid.NewGuid();
        _currentUser.Setup(x => x.UserId).Returns(_userId);

        var communityInteractionsRepo = new Mock<ICommunityInteractionRepository>();
        communityInteractionsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<CommunityInteraction>().AsQueryable().BuildMock());
        _uow.Setup(x => x.CommunityInteractions).Returns(communityInteractionsRepo.Object);
    }

    [Test]
    public async Task GetTemplateById_Should_Return_Template_When_Found()
    {
        var templateId = Guid.NewGuid();
        var creator = new User { Id = _userId, Name = "Alice" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Apparel" };
        var templates = new List<Template>
        {
            new()
            {
                Id = templateId,
                Name = "Cool Tee",
                PreviewImageURL = "/preview.png",
                CreatorUserId = _userId,
                CreatorUser = creator,
                CategoryId = category.Id,
                Category = category,
                IsPublic = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new GetTemplateByIdQueryHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new GetTemplateByIdQuery(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Id, Is.EqualTo(templateId));
        Assert.That(result.Data.Name, Is.EqualTo("Cool Tee"));
        Assert.That(result.Data.CreatorName, Is.EqualTo("Alice"));
        Assert.That(result.Data.CategoryName, Is.EqualTo("Apparel"));
    }

    [Test]
    public async Task GetTemplateById_Should_Return_NotFound_When_Missing()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Template>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new GetTemplateByIdQueryHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new GetTemplateByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task GetPublicTemplates_Should_Return_Paginated_Ordered_By_Likes()
    {
        var creator = new User { Id = Guid.NewGuid(), Name = "Bob" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Sports" };
        var templates = new List<Template>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Popular",
                PreviewImageURL = "/a.png",
                CreatorUser = creator,
                Category = category,
                IsPublic = true,
                IsDeleted = false,
                LikesCount = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Less Popular",
                PreviewImageURL = "/b.png",
                CreatorUser = creator,
                Category = category,
                IsPublic = true,
                IsDeleted = false,
                LikesCount = 10,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Private",
                PreviewImageURL = "/c.png",
                CreatorUser = creator,
                Category = category,
                IsPublic = false,
                IsDeleted = false,
                LikesCount = 500,
                CreatedAt = DateTime.UtcNow
            }
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new GetPublicTemplatesQueryHandler(_uow.Object);
        var result = await handler.Handle(new GetPublicTemplatesQuery(1, 1), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data.Data[0].Name, Is.EqualTo("Popular"));
        Assert.That(result.Data.TotalCount, Is.EqualTo(2));
        Assert.That(result.Data.CurrentPage, Is.EqualTo(1));
        Assert.That(result.Data.PageSize, Is.EqualTo(1));
    }

    [Test]
    public async Task GetMyTemplates_Should_Return_User_Templates()
    {
        var creator = new User { Id = _userId, Name = "Me" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Casual" };
        var templates = new List<Template>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "My Template",
                PreviewImageURL = "/mine.png",
                CreatorUserId = _userId,
                CreatorUser = creator,
                Category = category,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Other Template",
                PreviewImageURL = "/other.png",
                CreatorUserId = Guid.NewGuid(),
                CreatorUser = new User { Id = Guid.NewGuid(), Name = "Other" },
                Category = category,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new GetMyTemplatesQueryHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new GetMyTemplatesQuery(1, 10), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Data, Has.Count.EqualTo(1));
        Assert.That(result.Data.Data[0].Name, Is.EqualTo("My Template"));
    }

    [Test]
    public async Task GetMyTemplates_Should_Return_Empty_When_None()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<Template>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new GetMyTemplatesQueryHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new GetMyTemplatesQuery(1, 10), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Data, Is.Empty);
        Assert.That(result.Data.TotalCount, Is.EqualTo(0));
    }
}
