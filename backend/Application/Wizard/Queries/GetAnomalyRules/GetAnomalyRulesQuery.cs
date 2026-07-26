using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatdanSathi.API.Application.Common.Interfaces;

namespace MatdanSathi.API.Application.Wizard.Queries.GetAnomalyRules;

public record GetAnomalyRulesQuery(string? AnomalyType = null) : IRequest<List<AnomalyRuleDto>>;

public record AnomalyRuleDto(
    string AnomalyType,
    string Title,
    string Description,
    string ApplicableForm,
    List<string> RequiredProofDocuments,
    List<string> VerificationSteps,
    bool DirectSubmissionAllowed
);

public class GetAnomalyRulesQueryHandler : IRequestHandler<GetAnomalyRulesQuery, List<AnomalyRuleDto>>
{
    private static readonly List<AnomalyRuleDto> AnomalyMatrix = new()
    {
        new AnomalyRuleDto(
            AnomalyType: "SurnameMismatch",
            Title: "Surname / Name Transliteration Mismatch",
            Description: "Occurs when voter's surname differs due to marriage, regional script transliteration (Devanagari/Urdu), or clerical spelling error in official rolls.",
            ApplicableForm: "Form 8 (Correction of Particulars)",
            RequiredProofDocuments: new List<string>
            {
                "Self-attested Aadhaar Card copy",
                "Marriage Registration Certificate (if applicable)",
                "Passport copy displaying full name",
                "Gazette Notification or Notarized Affidavit on Rs 100 Stamp Paper"
            },
            VerificationSteps: new List<string>
            {
                "1. Cross-reference original regional script spelling.",
                "2. Upload self-attested identity proof with correct surname.",
                "3. Submit Form 8 online via voters.eci.gov.in portal link."
            },
            DirectSubmissionAllowed: true
        ),

        new AnomalyRuleDto(
            AnomalyType: "TemporaryAbsence",
            Title: "Marked Absent / Shifted Elector Notice",
            Description: "Occurs when BLO marked voter as 'Absent' or 'Shifted' during SIR door-to-door verification drive due to temporary employment or travel.",
            ApplicableForm: "Form 8 (Shifting of Residence / Verification)",
            RequiredProofDocuments: new List<string>
            {
                "Latest Electricity Bill or Water Bill in voter or spouse's name",
                "Registered Lease / Rent Agreement or House Ownership Deed",
                "Bank Passbook statement displaying current residential address"
            },
            VerificationSteps: new List<string>
            {
                "1. Log physical notice slip number left by visiting BLO.",
                "2. Schedule follow-up appointment time slot with assigned BLO.",
                "3. Present current residence utility proof to remove 'Shifted' tag."
            },
            DirectSubmissionAllowed: true
        ),

        new AnomalyRuleDto(
            AnomalyType: "ProgenyLinking",
            Title: "Progeny Linking / Ancestral Tagging",
            Description: "Establishes linkage to parent/ancestor registration records (e.g. 1995/2002 electoral roll archives) for first-time electors or heritage verification.",
            ApplicableForm: "Form 6 (New Elector) / Form 8 (Relationship Tagging)",
            RequiredProofDocuments: new List<string>
            {
                "Birth Certificate issuing parent's name",
                "Secondary School Leaving Certificate (SLC / Matriculation)",
                "Parent's EPIC Voter Card copy",
                "Certified Extract of 1995 / 2002 Electoral Roll"
            },
            VerificationSteps: new List<string>
            {
                "1. Search 2002 Archival Roll for parent/grandparent archival ID.",
                "2. Select parent EPIC ID for relationship link.",
                "3. Attach birth certificate displaying parentage."
            },
            DirectSubmissionAllowed: true
        ),

        new AnomalyRuleDto(
            AnomalyType: "DeceasedDeletion",
            Title: "Deceased Family Member Roll Deletion",
            Description: "Official objection and removal request for deceased family members to clean electoral rolls and prevent unauthorized proxy voting.",
            ApplicableForm: "Form 7 (Objection / Deletion Notice)",
            RequiredProofDocuments: new List<string>
            {
                "Death Certificate issued by Municipal Corporation (BMC/PMC)",
                "Burial / Cremation Ground Receipt",
                "Applicant's own EPIC Identity Card"
            },
            VerificationSteps: new List<string>
            {
                "1. Enter deceased relative's EPIC number or legacy ID.",
                "2. Attach municipal death registration certificate copy.",
                "3. Submit Form 7 notice package to Ward Office / BLO."
            },
            DirectSubmissionAllowed: true
        ),

        new AnomalyRuleDto(
            AnomalyType: "PhotoQualityImpairment",
            Title: "Photo Impairment / Demographic Update",
            Description: "Applies to legacy voter cards with low-resolution, blurred, or corrupted photographs.",
            ApplicableForm: "Form 8 (Replacement EPIC / Photo Update)",
            RequiredProofDocuments: new List<string>
            {
                "Recent Passport-size photograph (White background)",
                "Self-attested identity proof (Aadhaar / Passport)"
            },
            VerificationSteps: new List<string>
            {
                "1. Upload high-resolution photo file.",
                "2. Request e-EPIC downloadable digital card replacement."
            },
            DirectSubmissionAllowed: true
        )
    };

    public Task<List<AnomalyRuleDto>> Handle(GetAnomalyRulesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AnomalyType))
        {
            return Task.FromResult(AnomalyMatrix);
        }

        var filtered = AnomalyMatrix
            .Where(r => r.AnomalyType.Equals(request.AnomalyType.Trim(), System.StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(filtered.Count > 0 ? filtered : AnomalyMatrix);
    }
}
