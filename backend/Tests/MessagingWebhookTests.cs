using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Voters.Queries.CheckVoterRegistration;
using MatdarSathi.API.Controllers.v1;
using MatdarSathi.API.Domain.Entities;
using MatdarSathi.API.Infrastructure.Common;
using MatdarSathi.API.Infrastructure.Messaging;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdarSathi.API.Tests;

public class MessagingWebhookTests
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
    public async Task TwilioMessagingChannel_Parses_Sms_WhatsApp_Notification_Correctly()
    {
        // Arrange
        var settings = Options.Create(new MessagingSettings());
        var logger = new LoggerFactory().CreateLogger<TwilioMessagingChannel>();
        var channel = new TwilioMessagingChannel(settings, logger);

        var smsPayload = new WebhookPayload("+19998887777", "Check EPIC ABC1234567", "SMS", "sid1");
        var waPayload = new WebhookPayload("whatsapp:+19998887777", "Check EPIC ABC1234567", "WhatsApp", "sid2");
        var notifPayload = new WebhookPayload("user-notif-101", "Check EPIC ABC1234567", "Notification", "sid3");

        // Act
        var smsMsg = await channel.ReceiveWebhookAsync(smsPayload);
        var waMsg = await channel.ReceiveWebhookAsync(waPayload);
        var notifMsg = await channel.ReceiveWebhookAsync(notifPayload);

        // Assert
        Assert.Equal(MessagingChannelType.Sms, smsMsg.ChannelType);
        Assert.Equal(MessagingChannelType.WhatsApp, waMsg.ChannelType);
        Assert.Equal(MessagingChannelType.Notification, notifMsg.ChannelType);
    }

    [Fact]
    public async Task TwilioMessagingChannel_SendMessageAsync_Succeeds()
    {
        // Arrange
        var settings = Options.Create(new MessagingSettings());
        var logger = new LoggerFactory().CreateLogger<TwilioMessagingChannel>();
        var channel = new TwilioMessagingChannel(settings, logger);

        // Act
        var result = await channel.SendMessageAsync("+19998887777", "Test Outbound", MessagingChannelType.WhatsApp);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.MessageId);
    }

    [Fact]
    public async Task MessagingWebhookController_ValidEpic_ExecutesQuery_ReturnsBilingualResponseWithConsent()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        var epicNumber = "XYZ9876543";
        var blindIndex = cryptoService.GenerateBlindIndex(epicNumber);

        // Seed a matching voter profile
        dbContext.VoterProfiles.Add(new VoterProfile
        {
            EpicNumberEncrypted = cryptoService.Encrypt(epicNumber),
            EpicNumberBlindIndex = blindIndex,
            FullNameEncrypted = cryptoService.Encrypt("Rajesh Sharma"),
            DateOfBirthEncrypted = cryptoService.Encrypt("1985-05-15"),
            BloContactEncrypted = cryptoService.Encrypt("+919876543210"),
            Age = 40,
            Gender = "Male",
            AssemblyConstituency = "182-Mumbai",
            PartNumber = "45",
            SectionNumber = "1",
            SerialNumber = 102,
            PollingStationName = "BMC School Hall",
            PollingStationLocation = "Masjid Bunder, Mumbai",
            HouseNo = "B-401",
            BloName = "Sanjay Patil"
        });
        await dbContext.SaveChangesAsync();

        var settings = Options.Create(new MessagingSettings());
        var loggerChannel = new LoggerFactory().CreateLogger<TwilioMessagingChannel>();
        var messagingChannel = new TwilioMessagingChannel(settings, loggerChannel);
        var mediator = new NativeMediator(new SingleScopeServiceProvider(dbContext, cryptoService));
        var loggerController = new LoggerFactory().CreateLogger<MessagingWebhookController>();

        var controller = new MessagingWebhookController(messagingChannel, mediator, loggerController)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var requestDto = new WebhookRequestDto("+19876543210", $"Check my voter registration XYZ9876543", "WhatsApp", "sid_test_100");

        // Act
        var actionResult = await controller.HandleInboundWebhook(requestDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var status = okResult.Value?.GetType().GetProperty("status")?.GetValue(okResult.Value)?.ToString();
        var reply = okResult.Value?.GetType().GetProperty("reply")?.GetValue(okResult.Value)?.ToString();

        Assert.Equal("Found", status);
        Assert.NotNull(reply);
        Assert.Contains("[FOUND]", reply);
        Assert.Contains("[सापडले]", reply);
        Assert.Contains("Rajesh Sharma", reply);
        Assert.Contains("Privacy & Consent", reply);
        Assert.Contains("गोपनीयता आणि संमती", reply);
    }

    [Fact]
    public async Task MessagingWebhookController_InvalidEpicOrNoBody_ReturnsGracefulNotice()
    {
        // Arrange
        var (dbContext, cryptoService) = CreateTestContext();
        var settings = Options.Create(new MessagingSettings());
        var loggerChannel = new LoggerFactory().CreateLogger<TwilioMessagingChannel>();
        var messagingChannel = new TwilioMessagingChannel(settings, loggerChannel);
        var mediator = new NativeMediator(new SingleScopeServiceProvider(dbContext, cryptoService));
        var loggerController = new LoggerFactory().CreateLogger<MessagingWebhookController>();

        var controller = new MessagingWebhookController(messagingChannel, mediator, loggerController)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var requestDto = new WebhookRequestDto("+19876543210", "Hello bot", "SMS", "sid_test_101");

        // Act
        var actionResult = await controller.HandleInboundWebhook(requestDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var status = okResult.Value?.GetType().GetProperty("status")?.GetValue(okResult.Value)?.ToString();
        Assert.Equal("InvalidEpicFormat", status);
    }

    [Fact]
    public void MessagingWebhookController_DecoratedWithStrictLimitRateLimiting()
    {
        // Arrange & Act
        var attributes = typeof(MessagingWebhookController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), true);

        // Assert
        Assert.NotEmpty(attributes);
        var rateLimitAttr = Assert.IsType<EnableRateLimitingAttribute>(attributes.First());
        Assert.Equal("strict-limit", rateLimitAttr.PolicyName);
    }
}

// SingleScopeServiceProvider helper for testing NativeMediator in isolated test environment
internal class SingleScopeServiceProvider : IServiceProvider
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICryptographyService _cryptoService;

    public SingleScopeServiceProvider(ApplicationDbContext dbContext, ICryptographyService cryptoService)
    {
        _dbContext = dbContext;
        _cryptoService = cryptoService;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IRequestHandler<CheckVoterRegistrationQuery, VoterRegistrationDto>))
        {
            return new CheckVoterRegistrationQueryHandler(_dbContext, _cryptoService);
        }
        return null;
    }
}
