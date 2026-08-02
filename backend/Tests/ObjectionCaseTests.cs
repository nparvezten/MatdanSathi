using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Escalation.Queries.GetDistrictEscalationContact;
using MatdarSathi.API.Application.Objections.Commands.CreateObjectionCase;
using MatdarSathi.API.Application.Objections.Commands.UpdateObjectionCaseStatus;
using MatdarSathi.API.Application.Objections.Queries.GetObjectionCaseById;
using MatdarSathi.API.Controllers.v1;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Domain.Enums;
using MatdarSathi.API.Infrastructure.Common;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdarSathi.API.Tests;

public class ObjectionCaseTests
{
    private (ApplicationDbContext dbContext, CryptographyService cryptoService) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var cryptoSettings = Options.Create(new CryptographySettings
        {
            EncryptionKey = "12345678901234567890123456789012",
            BlindIndexSalt = "test-salt-secret-key-123456"
        });

        var cryptoService = new CryptographyService(cryptoSettings);
        var dbContext = new ApplicationDbContext(options, cryptoService);
        return (dbContext, cryptoService);
    }

    [Fact]
    public void ValidateStatusTransition_ValidTransitions_Succeed()
    {
        // Act & Assert (No exception thrown for valid transitions)
        UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.Draft, ObjectionStatus.Filed);
        UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.Filed, ObjectionStatus.Acknowledged);
        UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.Acknowledged, ObjectionStatus.UnderReview);
        UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.UnderReview, ObjectionStatus.Resolved);
        UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.UnderReview, ObjectionStatus.Rejected);
    }

    [Fact]
    public void ValidateStatusTransition_DraftToResolved_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            UpdateObjectionCaseStatusCommandHandler.ValidateStatusTransition(ObjectionStatus.Draft, ObjectionStatus.Resolved));

        Assert.Contains("Invalid status transition", ex.Message);
    }

    [Fact]
    public async Task CreateAndPatchObjectionCase_Flow_Succeeds()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        var handlerCreate = new CreateObjectionCaseCommandHandler(dbContext);
        var handlerUpdate = new UpdateObjectionCaseStatusCommandHandler(dbContext, cryptoService);

        var createCmd = new CreateObjectionCaseCommand(
            CaseType: ObjectionCaseType.Deletion,
            ApplicantName: "Sunita Deshmukh",
            EpicNumber: "ABC1234567",
            InitialNotes: "Duplicate voter entry reported on draft roll.");

        // Act 1: Create Case (Filed status)
        var createdDto = await handlerCreate.Handle(createCmd, CancellationToken.None);

        // Assert 1
        Assert.NotNull(createdDto);
        Assert.Equal(ObjectionStatus.Filed, createdDto.Status);
        Assert.Equal("Sunita Deshmukh", createdDto.ApplicantName);

        // Act 2: Transition Filed -> UnderReview
        var updateCmd = new UpdateObjectionCaseStatusCommand(
            Id: createdDto.Id,
            NewStatus: ObjectionStatus.UnderReview,
            EroNotes: "Hearing scheduled by ERO.");

        var updatedDto = await handlerUpdate.Handle(updateCmd, CancellationToken.None);

        // Assert 2
        Assert.Equal(ObjectionStatus.UnderReview, updatedDto.Status);
        Assert.Equal("Hearing scheduled by ERO.", updatedDto.EroNotes);
    }

    [Fact]
    public async Task EscalationController_KnownDistrict_Returns200WithData()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        dbContext.DistrictEscalationContacts.Add(new DistrictEscalationContact
        {
            District = "Mumbai City",
            EroNameOffice = "ERO Office 182-Mumbai City",
            DeoOfficeAddress = "Old Custom House, Fort",
            HelplineNumber = "1950",
            OfficialPortalUrl = "https://mumbaicity.gov.in/"
        });
        await dbContext.SaveChangesAsync();

        var mediator = new NativeMediator(new ObjectionTestServiceProvider(dbContext, cryptoService));
        var controller = new EscalationController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var result = await controller.GetEscalationContact("Mumbai City");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<DistrictEscalationContactDto>(okResult.Value);
        Assert.Equal("Mumbai City", dto.District);
        Assert.Equal("1950", dto.HelplineNumber);
    }

    [Fact]
    public async Task EscalationController_UnknownDistrict_Returns404NotFound()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        var mediator = new NativeMediator(new ObjectionTestServiceProvider(dbContext, cryptoService));
        var controller = new EscalationController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var result = await controller.GetEscalationContact("NonExistentDistrict");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

internal class ObjectionTestServiceProvider : IServiceProvider
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICryptographyService _cryptoService;

    public ObjectionTestServiceProvider(ApplicationDbContext dbContext, ICryptographyService cryptoService)
    {
        _dbContext = dbContext;
        _cryptoService = cryptoService;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IRequestHandler<GetDistrictEscalationContactQuery, DistrictEscalationContactDto?>))
        {
            return new GetDistrictEscalationContactQueryHandler(_dbContext);
        }
        return null;
    }
}
