using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.DTOS.CommunityDTOs;
using ITIGraduationProject.Application.Features.Community.Queries.Handlers;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.Products;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.Community;

[TestFixture]
public class CommunityQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;

    private GetCommunityFeedQueryHandler _feedHandler = null!;
    private GetTopCreatorsQueryHandler _topCreatorsHandler = null!;
    private GetModerationReportsQueryHandler _reportsHandler = null!;
    private GetTemplateCommentsQueryHandler _commentsHandler = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<CommunityInteraction, CommentDto>();

        TypeAdapterConfig.GlobalSettings.NewConfig<ModerationReport, ModerationReportDto>();

        TypeAdapterConfig.GlobalSettings.NewConfig<Template, FeedItemDto>();

        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _currentUser = new Mock<ICurrentUserService>();

        var communityInteractionsRepo = new Mock<ICommunityInteractionRepository>();
        communityInteractionsRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<CommunityInteraction>().AsQueryable().BuildMock());
        _uow.Setup(x => x.CommunityInteractions).Returns(communityInteractionsRepo.Object);

        _feedHandler = new GetCommunityFeedQueryHandler(_uow.Object, _currentUser.Object);
        _topCreatorsHandler = new GetTopCreatorsQueryHandler(_uow.Object);
        _reportsHandler = new GetModerationReportsQueryHandler(_uow.Object);
        _commentsHandler = new GetTemplateCommentsQueryHandler(_uow.Object, _currentUser.Object);
    }

    [Test]
    public async Task GetCommunityFeed_Should_Return_Public_Templates()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Fatma",
            ProfileImageUrl = "img.png"
        };

        var templates = new List<Template>
        {
            new()
            {
                Id=Guid.NewGuid(),
                Name="T1",
                CreatorUser=user,
                CreatorUserId=user.Id,
                IsPublic=true,
                LikesCount=20,
                IsDeleted=false
            },
            new()
            {
                Id=Guid.NewGuid(),
                Name="T2",
                CreatorUser=user,
                CreatorUserId=user.Id,
                IsPublic=false,
                IsDeleted=false
            }
        };

        var repo = new Mock<ITemplateRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());

        _uow.Setup(x => x.Templates).Returns(repo.Object);

        var result = await _feedHandler.Handle(
            new GetCommunityFeedQuery(1, 10, "popular"),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.Data.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTopCreators_Should_Return_Creators()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Fatma",
            ProfileImageUrl = "img.png"
        };

        var templates = new List<Template>
        {
            new()
            {
                Id=Guid.NewGuid(),
                CreatorUser=user,
                CreatorUserId=user.Id,
                IsPublic=true,
                LikesCount=5,
                RemixesCount=2,
                IsDeleted=false
            },
            new()
            {
                Id=Guid.NewGuid(),
                CreatorUser=user,
                CreatorUserId=user.Id,
                IsPublic=true,
                LikesCount=3,
                RemixesCount=1,
                IsDeleted=false
            }
        };

        var repo = new Mock<ITemplateRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(templates.AsQueryable().BuildMock());

        _uow.Setup(x => x.Templates).Returns(repo.Object);

        var result = await _topCreatorsHandler.Handle(
            new GetTopCreatorsQuery(10),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.Count, Is.EqualTo(1));
        Assert.That(result.Data[0].TemplateCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetModerationReports_Should_Return_Filtered_Data()
    {
        var reports = new List<ModerationReport>
{
    new ModerationReport
    {
        Id = Guid.NewGuid(),
        Status = ModerationReportStatus.Pending,
        Reason = "Spam",
        CreatedAt = DateTime.UtcNow,
        ReporterUserId = Guid.NewGuid(),
        TargetTemplateId = Guid.NewGuid(),

        ReporterUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Fatma"
        },

        TargetTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Template 1"
        }
    },

    new ModerationReport
    {
        Id = Guid.NewGuid(),
        Status = ModerationReportStatus.ActionTaken,
        Reason = "Bad Content",
        CreatedAt = DateTime.UtcNow,
        ReporterUserId = Guid.NewGuid(),
        TargetTemplateId = Guid.NewGuid(),

        ReporterUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Ali"
        },

        TargetTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Template 2"
        }
    }
};
        var repo = new Mock<IModerationReportRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(reports.AsQueryable().BuildMock());

        _uow.Setup(x => x.ModerationReports)
            .Returns(repo.Object);

        var result = await _reportsHandler.Handle(
            new GetModerationReportsQuery( 1, 10, ModerationReportStatus.Pending),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.Data.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTemplateComments_Should_Return_Comments()
    {
        var templateId = Guid.NewGuid();

        var comments = new List<CommunityInteraction>
{
    new CommunityInteraction
    {
        Id = Guid.NewGuid(),
        TemplateId = templateId,
        InteractionType = InteractionType.Comment,
        Content = "First",
        CreatedAt = DateTime.UtcNow,
        User = new User
        {
            Name = "Fatma",
            ProfileImageUrl = "img.png"
        }
    },

    new CommunityInteraction
    {
        Id = Guid.NewGuid(),
        TemplateId = templateId,
        InteractionType = InteractionType.Like,
        CreatedAt = DateTime.UtcNow,
        User = new User()
    },

    new CommunityInteraction
    {
        Id = Guid.NewGuid(),
        TemplateId = Guid.NewGuid(),
        InteractionType = InteractionType.Comment,
        CreatedAt = DateTime.UtcNow,
        User = new User()
    }
};

        var repo = new Mock<ICommunityInteractionRepository>();

        repo.Setup(x => x.GetTableNoTracking())
            .Returns(comments.AsQueryable().BuildMock());

        _uow.Setup(x => x.CommunityInteractions)
            .Returns(repo.Object);

        var result = await _commentsHandler.Handle(
            new GetTemplateCommentsQuery(templateId, 1, 10),
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data.Data.Count, Is.EqualTo(1));
        Assert.That(result.Data.Data.First().Content, Is.EqualTo("First"));
    }
}