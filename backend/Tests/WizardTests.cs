using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Wizard.Commands.GenerateHearingDossier;
using MatdarSathi.API.Application.Wizard.Models;
using MatdarSathi.API.Application.Wizard.Queries.GetAnomalyGuidance;
using Xunit;

namespace MatdarSathi.API.Tests;

public class WizardTests
{
    [Fact]
    public async Task GetAnomalyGuidance_Pre1987_ReturnsSingleSelfProofRequirement()
    {
        // Arrange
        var handler = new GetAnomalyGuidanceQueryHandler();
        var query = new GetAnomalyGuidanceQuery(Age: 42, BirthYear: 1982, AnomalyType: "SurnameMarriageChange");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pre1987", result.CitizenshipEra);
        Assert.Equal(1, result.RequiredSelfProofCount);
        Assert.Equal(0, result.RequiredFatherProofCount);
        Assert.Contains("Form 8", result.ApplicableForm);
    }

    [Fact]
    public async Task GetAnomalyGuidance_Between1987And2004_ReturnsParentProofRequirement()
    {
        // Arrange
        var handler = new GetAnomalyGuidanceQueryHandler();
        var query = new GetAnomalyGuidanceQuery(Age: 28, BirthYear: 1996, AnomalyType: "ProgenyMismatch");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Between1987And2004", result.CitizenshipEra);
        Assert.Equal(1, result.RequiredSelfProofCount);
        Assert.Equal(1, result.RequiredFatherProofCount);
    }

    [Fact]
    public async Task GenerateHearingDossier_ValidCommand_ReturnsFormattedDossier()
    {
        // Arrange
        var handler = new GenerateHearingDossierCommandHandler();
        var command = new GenerateHearingDossierCommand(
            VoterName: "Khan Saidnabi",
            EpicNumber: "SLD1234567",
            AssemblyConstituency: "Constituency 1",
            PollingStation: "Primary School Facility",
            AnomalyType: "SurnameMarriageChange",
            CitizenshipEra: "Pre1987",
            SelectedSelfProofs: new List<string> { "Aadhaar Card", "Marriage Certificate" },
            SelectedParentProofs: new List<string>(),
            HearingBoothLocation: "Local ERO Office"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("AERO-DOSSIER-", result.DossierReference);
        Assert.True(result.IsReadyForPrint);
        Assert.Contains("Khan Saidnabi", result.HearingNoticeText);
        Assert.Contains("SLD1234567", result.HearingNoticeText);
    }
}
