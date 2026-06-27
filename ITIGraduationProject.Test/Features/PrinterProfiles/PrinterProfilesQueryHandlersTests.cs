using ITIGraduationProject.Application.Features.PrinterProfiles.Mapping;
using ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Handlers;
using ITIGraduationProject.Application.Features.PrinterProfiles.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MapsterMapper;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace ITIGraduationProject.Test.Features.PrinterProfiles;

[TestFixture]
public class PrinterProfilesQueryHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private static IMapper _mapper = null!;
    private GetPrinterProfilesQueryHandler _handler = null!;

    [OneTimeSetUp]
    public void RegisterMapster()
    {
        var config = new TypeAdapterConfig();
        new PrinterProfileMappingConfig().Register(config);
        config.Compile();
        _mapper = new Mapper(config);
    }

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _handler = new GetPrinterProfilesQueryHandler(_uow.Object, _mapper);
    }

    [Test]
    public async Task GetPrinterProfiles_Should_Return_Data()
    {
        var profiles = new List<PrinterProfile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SupportedFabrics = FabricType.Cotton,
                SupportedPrintMethods = PrintMethodType.DirectToGarment,
                IsActive = true,
                IsDeleted = false,
                User = new User { Name = "Printer Alpha" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SupportedFabrics = FabricType.Silk,
                SupportedPrintMethods = PrintMethodType.Sublimation,
                IsActive = true,
                IsDeleted = false,
                User = new User { Name = "Printer Beta" }
            }
        };

        var repo = new Mock<IPrinterProfileRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(profiles.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(repo.Object);

        var result = await _handler.Handle(
            new GetPrinterProfilesQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.Data[0].PrinterName, Is.EqualTo("Printer Alpha"));
        Assert.That(result.Data[1].PrinterName, Is.EqualTo("Printer Beta"));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetPrinterProfiles_Should_Return_Paginated_Results()
    {
        var profiles = Enumerable.Range(1, 5)
            .Select(i => new PrinterProfile
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SupportedFabrics = FabricType.Cotton,
                SupportedPrintMethods = PrintMethodType.HeatTransfer,
                IsActive = true,
                IsDeleted = false,
                User = new User { Name = $"Printer {i}" }
            })
            .ToList();

        var repo = new Mock<IPrinterProfileRepository>();
        repo.Setup(x => x.GetTableNoTracking())
            .Returns(profiles.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(repo.Object);

        var result = await _handler.Handle(
            new GetPrinterProfilesQuery { PageNumber = 2, PageSize = 2 },
            CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.CurrentPage, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.Data[0].PrinterName, Is.EqualTo("Printer 3"));
        Assert.That(result.Data[1].PrinterName, Is.EqualTo("Printer 4"));
    }
}
