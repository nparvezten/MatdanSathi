using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Wizard.Models;

namespace MatdarSathi.API.Application.Wizard.Queries.GetAnomalyGuidance;

public record GetAnomalyGuidanceQuery(
    int Age,
    int? BirthYear,
    string AnomalyType) : IRequest<GuidanceResponseDto>;

public class GetAnomalyGuidanceQueryHandler : IRequestHandler<GetAnomalyGuidanceQuery, GuidanceResponseDto>
{
    public static readonly List<DocumentRule> Official12ProofDocuments = new()
    {
        new DocumentRule(1, "PASSPORT", "Indian Passport", "Valid Indian Passport displaying date of birth and parentage.", "National Identity", true, true),
        new DocumentRule(2, "BIRTH_CERT", "Municipal Birth Certificate", "Birth Certificate issued by Municipal Registrar or District Authority.", "Civil Status", true, true),
        new DocumentRule(3, "SCHOOL_CERT", "Class 10 / School Leaving Certificate (SLC)", "Matriculation certificate, TC, or Board marksheet showing DOB.", "Educational Record", true, true),
        new DocumentRule(4, "GOVT_PPO", "Govt ID / Pension Payment Order (PPO)", "Official Government Employee ID or Ex-Servicemen Pension Order.", "Official Service", true, true),
        new DocumentRule(5, "LAND_DEED", "Land Allotment / House Title Deed", "Registered land deed or municipal property allotment order.", "Property & Residence", true, true),
        new DocumentRule(6, "AADHAAR", "Aadhaar Card", "UIDAI Aadhaar Card displaying full DOB.", "Biometric Identity", true, true),
        new DocumentRule(7, "CASTE_CERT", "Caste Certificate", "State/Central Govt Caste Certificate issued by Competent Authority.", "Official Certificate", true, true),
        new DocumentRule(8, "PRC_DOMICILE", "Permanent Resident / Domicile Certificate", "State Domicile or Permanent Residence Certificate.", "State Residency", true, true),
        new DocumentRule(9, "FOREST_RIGHTS", "Forest Rights Certificate", "Certificate issued under Scheduled Tribes & Forest Dwellers Act.", "Customary Rights", true, true),
        new DocumentRule(10, "FAMILY_REGISTER", "Family Register / Gram Panchayat Extract", "Certified extract from Village Family Register or Municipal Register.", "Local Revenue", true, true),
        new DocumentRule(11, "PRE_1987_DOC", "Pre-1987 Government Treasury/Service Record", "Pre-1987 land revenue receipt, treasury record, or Govt service register.", "Legacy Archive", true, true),
        new DocumentRule(12, "NRC_LEGACY", "NRC / Archival Electoral Roll Extract", "Certified extract of 1995 or 2002 Electoral Roll / Legacy Data.", "Electoral Heritage", true, true)
    };

    public Task<GuidanceResponseDto> Handle(GetAnomalyGuidanceQuery request, CancellationToken cancellationToken)
    {
        int currentYear = DateTime.UtcNow.Year;
        int calculatedBirthYear = request.BirthYear ?? (currentYear - (request.Age > 0 ? request.Age : 30));

        // Determine Citizenship Era cutoff rule under ECI SIR guidelines
        CitizenshipEra era;
        int selfCount, fatherCount, motherCount;
        string eraDescription, ageCategoryLabel;

        if (calculatedBirthYear < 1987)
        {
            era = CitizenshipEra.Pre1987;
            selfCount = 1;
            fatherCount = 0;
            motherCount = 0;
            ageCategoryLabel = "Born before July 1, 1987 (Pre-1987 Era)";
            eraDescription = "Under ECI SIR Rules, electors born before 01.07.1987 require 1 valid proof document for Self.";
        }
        else if (calculatedBirthYear <= 2004)
        {
            era = CitizenshipEra.Between1987And2004;
            selfCount = 1;
            fatherCount = 1; // 1 for Self + 1 for Father OR Mother
            motherCount = 0;
            ageCategoryLabel = "Born between July 1, 1987 and Dec 2, 2004";
            eraDescription = "Under ECI SIR Rules, electors born between 01.07.1987 and 02.12.2004 require 1 proof for Self + 1 proof for Father or Mother.";
        }
        else
        {
            era = CitizenshipEra.Post2004;
            selfCount = 1;
            fatherCount = 1;
            motherCount = 1;
            ageCategoryLabel = "Born after Dec 2, 2004 (Post-2004 Era)";
            eraDescription = "Under ECI SIR Rules, electors born after 02.12.2004 require 1 proof for Self + 1 proof for Father + 1 proof for Mother.";
        }

        // Map applicable ECI form type based on anomaly type
        string applicableForm = GetApplicableForm(request.AnomalyType);

        var checklist = new List<string>
        {
            $"1. Verify selected Anomaly: {request.AnomalyType}",
            $"2. Gather {selfCount} document for Self" + (fatherCount > 0 ? $" and {fatherCount} document for Parent" : ""),
            $"3. Submit {applicableForm} online or present physical copy at AERO Hearing Camp."
        };

        var response = new GuidanceResponseDto(
            AnomalyType: request.AnomalyType,
            CitizenshipEra: era.ToString(),
            BirthYear: calculatedBirthYear,
            AgeCategoryLabel: ageCategoryLabel,
            EciCutoffRuleDescription: eraDescription,
            RequiredSelfProofCount: selfCount,
            RequiredFatherProofCount: fatherCount,
            RequiredMotherProofCount: motherCount,
            ApplicableForm: applicableForm,
            EligibleSelfDocuments: Official12ProofDocuments.Where(d => d.ValidForSelf).ToList(),
            EligibleParentDocuments: Official12ProofDocuments.Where(d => d.ValidForParent).ToList(),
            ActionChecklist: checklist
        );

        return Task.FromResult(response);
    }

    private static string GetApplicableForm(string anomalyType)
    {
        return anomalyType switch
        {
            "Unmapped2002Record" => "Form 6 (New Registration) / Form 8 (Archival Tagging)",
            "ProgenyMismatch" => "Form 6 (Parentage Tagging)",
            "SurnameMarriageChange" => "Form 8 (Correction of Particulars)",
            "AgeDobConflict" => "Form 8 (DOB Correction)",
            "DoorLockedShifted" => "Form 8 (Shifting of Residence / Verification)",
            "Form7Objection" => "Form 7 (Objection / Deletion Notice)",
            "AddressTransfer" => "Form 8 (Address Shifting)",
            _ => "Form 8 (Correction)"
        };
    }
}
