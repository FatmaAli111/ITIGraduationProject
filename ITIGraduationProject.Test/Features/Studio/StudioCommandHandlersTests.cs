using ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatMessage;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatSession;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset;
using ITIGraduationProject.Application.Features.Studio.Commands.DeleteDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.DeleteGraphicAsset;
using ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Designs;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Studio;

[TestFixture]
public class StudioCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<ISender> _sender = null!;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();
        _sender = new Mock<ISender>();
        _userId = Guid.NewGuid();

        _currentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Test]
    public async Task CreateGraphicAsset_Should_Add_And_Save()
    {
        var graphicRepo = new Mock<IGraphicAssetRepository>();
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateGraphicAssetCommandHandler(_uow.Object);
        var command = new CreateGraphicAssetCommand(
            "Logo",
            GraphicAssetType.UploadedImage,
            "/uploads/logo.png",
            "brand",
            _userId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        graphicRepo.Verify(r => r.AddAsync(It.Is<GraphicAsset>(a =>
            a.Name == "Logo" &&
            a.UserId == _userId &&
            a.Type == GraphicAssetType.UploadedImage)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteDesign_Should_Delete_When_Found()
    {
        var designId = Guid.NewGuid();
        var designs = new List<Design>
        {
            new()
            {
                Id = designId,
                UserId = _userId,
                ProductId = Guid.NewGuid(),
                CanvasStateJSON = "{}",
                Status = DesignStatus.Draft
            }
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableAsTracking())
            .Returns(designs.AsQueryable().BuildMock());
        designRepo.Setup(x => x.Delete(It.IsAny<Design>()));

        _uow.Setup(x => x.Designs).Returns(designRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteDesignCommandHandler(_uow.Object);
        await handler.Handle(new DeleteDesignCommand(designId), CancellationToken.None);

        designRepo.Verify(x => x.Delete(It.Is<Design>(d => d.Id == designId)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteDesign_Should_Throw_When_Not_Found()
    {
        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<Design>().AsQueryable().BuildMock());
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new DeleteDesignCommandHandler(_uow.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteDesignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Test]
    public async Task DeleteGraphicAsset_Should_Delete_When_Found()
    {
        var assetId = Guid.NewGuid();
        var assets = new List<GraphicAsset>
        {
            new()
            {
                Id = assetId,
                UserId = _userId,
                Name = "Sticker",
                Type = GraphicAssetType.Sticker,
                ImageUrl = "/img.png"
            }
        };

        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableAsTracking())
            .Returns(assets.AsQueryable().BuildMock());
        graphicRepo.Setup(x => x.Delete(It.IsAny<GraphicAsset>()));

        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new DeleteGraphicAssetCommandHandler(_uow.Object);
        await handler.Handle(new DeleteGraphicAssetCommand(assetId), CancellationToken.None);

        graphicRepo.Verify(x => x.Delete(It.Is<GraphicAsset>(a => a.Id == assetId)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteGraphicAsset_Should_Throw_When_Not_Found()
    {
        var graphicRepo = new Mock<IGraphicAssetRepository>();
        graphicRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<GraphicAsset>().AsQueryable().BuildMock());
        _uow.Setup(x => x.GraphicAssets).Returns(graphicRepo.Object);

        var handler = new DeleteGraphicAssetCommandHandler(_uow.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteGraphicAssetCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Test]
    public async Task CreateAiChatSession_Should_Add_And_Save()
    {
        var sessionRepo = new Mock<IAiChatSessionRepository>();
        _uow.Setup(x => x.AiChatSessions).Returns(sessionRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var productId = Guid.NewGuid();
        var handler = new CreateAiChatSessionCommandHandler(_uow.Object);
        var result = await handler.Handle(
            new CreateAiChatSessionCommand(_userId, productId),
            CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        sessionRepo.Verify(r => r.AddAsync(It.Is<AiChatSession>(s =>
            s.UserId == _userId &&
            s.CurrentDesignId == productId)), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAiChatMessage_Should_Add_Messages_When_Session_Exists()
    {
        var sessionId = Guid.NewGuid();
        var session = new AiChatSession
        {
            Id = sessionId,
            UserId = _userId,
            AiChatMessages = new List<AiChatMessage>()
        };

        var sessionRepo = new Mock<IAiChatSessionRepository>();
        sessionRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<AiChatSession> { session }.AsQueryable().BuildMock());
        sessionRepo.Setup(x => x.Update(It.IsAny<AiChatSession>()));

        _uow.Setup(x => x.AiChatSessions).Returns(sessionRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateAiChatMessageCommandHandler(_uow.Object);
        var result = await handler.Handle(
            new CreateAiChatMessageCommand(sessionId, "Hello AI"),
            CancellationToken.None);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        Assert.That(session.AiChatMessages.Count, Is.EqualTo(2));
        sessionRepo.Verify(x => x.Update(session), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAiChatMessage_Should_Throw_When_Session_Not_Found()
    {
        var sessionRepo = new Mock<IAiChatSessionRepository>();
        sessionRepo.Setup(x => x.GetTableAsTracking())
            .Returns(new List<AiChatSession>().AsQueryable().BuildMock());
        _uow.Setup(x => x.AiChatSessions).Returns(sessionRepo.Object);

        var handler = new CreateAiChatMessageCommandHandler(_uow.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CreateAiChatMessageCommand(Guid.NewGuid(), "Hi"), CancellationToken.None));
    }

    [Test]
    public async Task UpdateDesign_Should_Delegate_To_Mediator_When_Authorized()
    {
        var designId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var design = new Design
        {
            Id = designId,
            UserId = _userId,
            ProductId = productId,
            CanvasStateJSON = "{\"v\":1}",
            Status = DesignStatus.Draft
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(designId)).ReturnsAsync(design);
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        _sender.Setup(s => s.Send(It.IsAny<CreateDesignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(designId);

        var handler = new UpdateDesignCommandHandler(_uow.Object, _currentUser.Object, _sender.Object);
        await handler.Handle(new UpdateDesignCommand(
            designId,
            "{\"v\":2}",
            null,
            null,
            null,
            ProductSize.L,
            FabricType.Cotton,
            PrintMethodType.DirectToGarment,
            "#000",
            null), CancellationToken.None);

        _sender.Verify(s => s.Send(
            It.Is<CreateDesignCommand>(c => c.Id == designId && c.ProductId == productId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void UpdateDesign_Should_Throw_When_Not_Found()
    {
        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Design?)null);
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new UpdateDesignCommandHandler(_uow.Object, _currentUser.Object, _sender.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateDesignCommand(
                Guid.NewGuid(), "{}", null, null, null, null, null, null, null, null),
                CancellationToken.None));
    }

    [Test]
    public void UpdateDesign_Should_Throw_When_Unauthorized()
    {
        var designId = Guid.NewGuid();
        var design = new Design
        {
            Id = designId,
            UserId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            CanvasStateJSON = "{}",
            Status = DesignStatus.Draft
        };

        var designRepo = new Mock<IDesignRepository>();
        designRepo.Setup(x => x.GetByIdAsync(designId)).ReturnsAsync(design);
        _uow.Setup(x => x.Designs).Returns(designRepo.Object);

        var handler = new UpdateDesignCommandHandler(_uow.Object, _currentUser.Object, _sender.Object);

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateDesignCommand(
                designId, "{}", null, null, null, null, null, null, null, null),
                CancellationToken.None));
    }
}
