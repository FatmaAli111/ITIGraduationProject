using ITIGraduationProject.Application.Bases.Templates;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using ITIGraduationProject.Application.Features.Templates.Commands.Handlers;
using ITIGraduationProject.Application.Features.Templates.Commands.Models;
using ITIGraduationProject.Application.Features.Templates.Mapping;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ITIGraduationProject.Test.Features.Templates;

[TestFixture]
public class TemplatesCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<IAILayerClient> _ai = null!;
    private Guid _userId;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        new TemplateMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
        TypeAdapterConfig.GlobalSettings.NewConfig<Template, TemplateDto>()
            .Map(dest => dest.CreatorName, src => src.CreatorUser != null ? src.CreatorUser.Name : string.Empty)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty);
        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();
        _ai = new Mock<IAILayerClient>();
        _userId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(_userId);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
    }

    [Test]
    public async Task CreateTemplate_Should_Add_And_Return_Dto()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new CreateTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(
            new CreateTemplateCommand("Summer Tee", Guid.NewGuid(), "casual", "/preview.png", false),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data!.Name, Is.EqualTo("Summer Tee"));
        templateRepo.Verify(r => r.AddAsync(It.Is<Template>(t =>
            t.CreatorUserId == _userId && t.Name == "Summer Tee")), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateTemplate_Should_Succeed_When_Owner()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = _userId,
            Name = "Old Name",
            PreviewImageURL = "/old.png",
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        templateRepo.Setup(r => r.Update(It.IsAny<Template>()));
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new UpdateTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(
            new UpdateTemplateCommand(templateId, "New Name", "sporty", null),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(template.Name, Is.EqualTo("New Name"));
        Assert.That(template.StyleTags, Is.EqualTo("sporty"));
        templateRepo.Verify(r => r.Update(template), Times.Once);
    }

    [Test]
    public async Task UpdateTemplate_Should_Return_NotFound_When_Missing()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Template?)null);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new UpdateTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(
            new UpdateTemplateCommand(Guid.NewGuid(), "Name", null, null),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task UpdateTemplate_Should_Return_Unauthorized_When_Not_Owner()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = Guid.NewGuid(),
            Name = "Other",
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new UpdateTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(
            new UpdateTemplateCommand(templateId, "Hack", null, null),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteTemplate_Should_SoftDelete_When_Owner()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = _userId,
            Name = "To Delete",
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        templateRepo.Setup(r => r.Update(It.IsAny<Template>()));
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new DeleteTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new DeleteTemplateCommand(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(template.IsDeleted, Is.True);
        Assert.That(template.DeletedAt, Is.Not.Null);
        templateRepo.Verify(r => r.Update(template), Times.Once);
    }

    [Test]
    public async Task DeleteTemplate_Should_Return_NotFound_When_Missing()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Template?)null);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new DeleteTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new DeleteTemplateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task DeleteTemplate_Should_Return_Unauthorized_When_Not_Owner()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = Guid.NewGuid(),
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new DeleteTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new DeleteTemplateCommand(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task PublishTemplate_Should_Succeed_When_Owner_And_Private()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = _userId,
            IsPublic = false,
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        templateRepo.Setup(r => r.Update(It.IsAny<Template>()));
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new PublishTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new PublishTemplateCommand(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(template.IsPublic, Is.True);
        templateRepo.Verify(r => r.Update(template), Times.Once);
    }

    [Test]
    public async Task PublishTemplate_Should_Return_NotFound_When_Missing()
    {
        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Template?)null);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new PublishTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new PublishTemplateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Template not found"));
    }

    [Test]
    public async Task PublishTemplate_Should_Return_Unauthorized_When_Not_Owner()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = Guid.NewGuid(),
            IsPublic = false,
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new PublishTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new PublishTemplateCommand(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task PublishTemplate_Should_Return_BadRequest_When_Already_Public()
    {
        var templateId = Guid.NewGuid();
        var template = new Template
        {
            Id = templateId,
            CreatorUserId = _userId,
            IsPublic = true,
            IsDeleted = false
        };

        var templateRepo = new Mock<ITemplateRepository>();
        templateRepo.Setup(r => r.GetByIdAsync(templateId)).ReturnsAsync(template);
        _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

        var handler = new PublishTemplateCommandHandler(_uow.Object, _currentUser.Object);
        var result = await handler.Handle(new PublishTemplateCommand(templateId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Template is already published"));
    }

    [Test]
    public async Task GenerateAITemplate_Should_Return_NotFound_When_Product_Missing()
    {
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var handler = new GenerateAITemplateCommandHandler(_uow.Object, _ai.Object, _currentUser.Object);
        var result = await handler.Handle(new GenerateAITemplateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Message, Is.EqualTo("Product not found"));
    }

    [Test]
    public async Task GenerateAITemplate_Should_Return_BadRequest_When_No_Preferences()
    {
        var productId = Guid.NewGuid();
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product
        {
            Id = productId,
            Name = "Tee",
            CategoryId = Guid.NewGuid()
        });
        _uow.Setup(x => x.Products).Returns(productRepo.Object);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetTableNoTracking())
            .Returns(new List<User>
            {
                new() { Id = _userId, UserPreferences = null! }
            }.AsQueryable().BuildMock());
        _uow.Setup(x => x.Users).Returns(userRepo.Object);

        var handler = new GenerateAITemplateCommandHandler(_uow.Object, _ai.Object, _currentUser.Object);
        var result = await handler.Handle(new GenerateAITemplateCommand(productId), CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Complete preference onboarding before generating templates"));
    }

    [Test]
    public async Task GenerateAITemplate_Should_Succeed_When_Product_And_Preferences_Exist()
    {
        const int port = 18765;
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

        var listenerTask = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync();
            context.Response.ContentType = "image/png";
            await context.Response.OutputStream.WriteAsync(pngBytes);
            context.Response.Close();
        });

        try
        {
            var productId = Guid.NewGuid();
            var productRepo = new Mock<IProductRepository>();
            productRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product
            {
                Id = productId,
                Name = "Tee",
                CategoryId = Guid.NewGuid(),
                Category = new Category { Name = "Shirts" }
            });
            _uow.Setup(x => x.Products).Returns(productRepo.Object);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetTableNoTracking())
                .Returns(new List<User>
                {
                    new()
                    {
                        Id = _userId,
                        UserPreferences = new UserPreferences
                        {
                            UserId = _userId,
                            StyleType = "Sporty",
                            FavoriteColors = "Blue",
                            Interests = "Gym",
                            DesignPreference = "light"
                        }
                    }
                }.AsQueryable().BuildMock());
            _uow.Setup(x => x.Users).Returns(userRepo.Object);

            var templateRepo = new Mock<ITemplateRepository>();
            _uow.Setup(x => x.Templates).Returns(templateRepo.Object);

            _ai.Setup(a => a.GenerateTemplateImageAsync(It.IsAny<AIGenerateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync($"http://127.0.0.1:{port}/test.png");

            var handler = new GenerateAITemplateCommandHandler(_uow.Object, _ai.Object, _currentUser.Object);
            var result = await handler.Handle(new GenerateAITemplateCommand(productId), CancellationToken.None);

            await listenerTask;

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data!.PreviewImageURL, Does.StartWith("/uploads/templates/"));
            templateRepo.Verify(r => r.AddAsync(It.IsAny<Template>()), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        finally
        {
            listener.Stop();
            listener.Close();
        }
    }
}
