using ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Handlers;
using ITIGraduationProject.Application.Features.PrinterProfiles.Commands.Models;
using ITIGraduationProject.Application.Features.PrinterProfiles.Mapping;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using MapsterMapper;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ITIGraduationProject.Test.Features.PrinterProfiles;

[TestFixture]
public class PrinterProfilesCommandHandlersTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private static IMapper _mapper = null!;
    private CreatePrinterProfileCommandHandler _handler = null!;

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
        _currentUser = new Mock<ICurrentUserService>();

        _handler = new CreatePrinterProfileCommandHandler(
            _uow.Object,
            _currentUser.Object,
            _mapper);
    }

    [Test]
    public async Task CreatePrinterProfile_Should_Succeed()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.UserId).Returns(userId);

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(new List<PrinterProfile>().AsQueryable().BuildMock());

        printerProfileRepo.Setup(x => x.AddAsync(It.IsAny<PrinterProfile>()))
            .Callback<PrinterProfile>(profile =>
            {
                profile.User = new User { Id = userId, Name = "Printer One" };
            })
            .ReturnsAsync((PrinterProfile profile) => profile);

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreatePrinterProfileCommand
        {
            SupportedFabrics = FabricType.Cotton | FabricType.Polyester,
            SupportedPrintMethods = PrintMethodType.DirectToGarment
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(result.Data.UserId, Is.EqualTo(userId));
        Assert.That(result.Data.SupportedFabrics, Is.EqualTo(FabricType.Cotton | FabricType.Polyester));
        Assert.That(result.Data.SupportedPrintMethods, Is.EqualTo(PrintMethodType.DirectToGarment));
        Assert.That(result.Data.IsActive, Is.True);
        Assert.That(result.Data.PrinterName, Is.EqualTo("Printer One"));

        printerProfileRepo.Verify(x => x.AddAsync(It.IsAny<PrinterProfile>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreatePrinterProfile_Should_Return_BadRequest_When_Already_Exists()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.UserId).Returns(userId);

        var existingProfiles = new List<PrinterProfile>
        {
            new()
            {
                UserId = userId,
                IsDeleted = false,
                IsActive = true
            }
        };

        var printerProfileRepo = new Mock<IPrinterProfileRepository>();
        printerProfileRepo.Setup(x => x.GetTableNoTracking())
            .Returns(existingProfiles.AsQueryable().BuildMock());

        _uow.Setup(x => x.PrinterProfiles).Returns(printerProfileRepo.Object);

        var command = new CreatePrinterProfileCommand
        {
            SupportedFabrics = FabricType.Cotton,
            SupportedPrintMethods = PrintMethodType.ScreenPrinting
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Message, Is.EqualTo("Printer profile already exists for this user"));
    }
}
