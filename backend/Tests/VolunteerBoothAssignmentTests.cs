using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MatdarSathi.API.Application.Ingestion.Commands.ClaimBooth;
using MatdarSathi.API.Application.Ingestion.Queries.GetBoothAssignments;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdarSathi.API.Tests;

public class VolunteerBoothAssignmentTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
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
        return new ApplicationDbContext(options, cryptoService);
    }

    [Fact]
    public async Task GetBoothAssignmentsQuery_ReturnsDefaultBooths()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new GetBoothAssignmentsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetBoothAssignmentsQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, b => b.BoothId == "BOOTH-101-WEST");
    }

    [Fact]
    public async Task ClaimBoothCommand_ValidBooth_ClaimsAssignment()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var claimHandler = new ClaimBoothCommandHandler(dbContext);
        var queryHandler = new GetBoothAssignmentsQueryHandler(dbContext);

        var claimCmd = new ClaimBoothCommand(
            BoothId: "BOOTH-101-WEST",
            VolunteerId: "vol-user-202",
            BoothName: "Primary School Room 1",
            AssemblyConstituency: "182-Mumbai");

        // Act 1: Claim booth
        var claimedDto = await claimHandler.Handle(claimCmd, CancellationToken.None);

        // Assert 1
        Assert.True(claimedDto.IsClaimed);
        Assert.Equal("vol-user-202", claimedDto.ClaimedByVolunteerId);

        // Act 2: Query booth status
        var boothList = await queryHandler.Handle(new GetBoothAssignmentsQuery(), CancellationToken.None);
        var booth101 = boothList.First(b => b.BoothId == "BOOTH-101-WEST");

        // Assert 2
        Assert.True(booth101.IsClaimed);
        Assert.Equal("vol-user-202", booth101.ClaimedByVolunteerId);
    }

    [Fact]
    public async Task ClaimBoothCommand_AlreadyClaimedByAnotherVolunteer_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var claimHandler = new ClaimBoothCommandHandler(dbContext);

        var claimCmd1 = new ClaimBoothCommand("BOOTH-101-WEST", "vol-user-1");
        var claimCmd2 = new ClaimBoothCommand("BOOTH-101-WEST", "vol-user-2");

        await claimHandler.Handle(claimCmd1, CancellationToken.None);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            claimHandler.Handle(claimCmd2, CancellationToken.None));

        Assert.Contains("already been claimed", ex.Message);
    }
}
