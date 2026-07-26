using System;
using System.Collections.Generic;

namespace MatdanSathi.API.Application.Wizard.Models;

public enum AnomalyType
{
    Unmapped2002Record,
    ProgenyMismatch,
    SurnameMarriageChange,
    AgeDobConflict,
    DoorLockedShifted,
    Form7Objection,
    AddressTransfer
}

public enum CitizenshipEra
{
    Pre1987,           // Born before 01.07.1987
    Between1987And2004, // Born between 01.07.1987 and 02.12.2004
    Post2004           // Born after 02.12.2004
}

public record DocumentRule(
    int RuleId,
    string Code,
    string Name,
    string Description,
    string Category,
    bool ValidForSelf,
    bool ValidForParent
);

public record GuidanceResponseDto(
    string AnomalyType,
    string CitizenshipEra,
    int BirthYear,
    string AgeCategoryLabel,
    string EciCutoffRuleDescription,
    int RequiredSelfProofCount,
    int RequiredFatherProofCount,
    int RequiredMotherProofCount,
    string ApplicableForm,
    List<DocumentRule> EligibleSelfDocuments,
    List<DocumentRule> EligibleParentDocuments,
    List<string> ActionChecklist
);

public record DossierResponseDto(
    string DossierReference,
    string GeneratedAt,
    string VoterName,
    string EpicNumber,
    string AssemblyConstituency,
    string PollingStation,
    string AnomalyType,
    string CitizenshipEra,
    string ApplicableForm,
    List<string> SelectedSelfProofs,
    List<string> SelectedParentProofs,
    string HearingBoothLocation,
    string HearingNoticeText,
    bool IsReadyForPrint
);
