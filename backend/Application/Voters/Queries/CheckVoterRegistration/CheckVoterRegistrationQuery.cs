using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Domain.Entities;

namespace MatdanSathi.API.Application.Voters.Queries.CheckVoterRegistration;

public record CheckVoterRegistrationQuery(
    string EpicNumber,
    string VerifierId,
    string VerificationMethod) : IRequest<VoterRegistrationDto>;

public class CheckVoterRegistrationQueryHandler : IRequestHandler<CheckVoterRegistrationQuery, VoterRegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public CheckVoterRegistrationQueryHandler(
        IApplicationDbContext context,
        ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    public async Task<VoterRegistrationDto> Handle(
        CheckVoterRegistrationQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Generate blind index of EPIC number for deterministic matching
        string blindIndex = _cryptographyService.GenerateBlindIndex(request.EpicNumber.Trim().ToUpperInvariant());

        // 2. Query database for matching blind index
        var voter = await _context.VoterProfiles
            .FirstOrDefaultAsync(v => v.EpicNumberBlindIndex == blindIndex && !v.IsDeleted, cancellationToken);

        if (voter == null)
        {
            // Log verification failure for security auditing (EPIC not found)
            var failLog = new VerificationLog
            {
                VerifierId = request.VerifierId,
                VerificationTimestamp = DateTimeOffset.UtcNow,
                IsVerified = false,
                VerificationMethod = request.VerificationMethod,
                Notes = $"Verification failed. EPIC card {request.EpicNumber} not found."
            };

            _context.VerificationLogs.Add(failLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new VoterRegistrationDto
            {
                EpicNumber = request.EpicNumber,
                IsVerified = false,
                FullName = "NOT FOUND"
            };
        }

        // 3. Decrypt PII fields for safe presentation
        string decryptedEpic = _cryptographyService.Decrypt(voter.EpicNumberEncrypted);
        string decryptedName = _cryptographyService.Decrypt(voter.FullNameEncrypted);
        string decryptedDob = _cryptographyService.Decrypt(voter.DateOfBirthEncrypted);
        string decryptedBloContact = _cryptographyService.Decrypt(voter.BloContactEncrypted);

        // 4. Log successful verification
        var successLog = new VerificationLog
        {
            VoterProfileId = voter.Id,
            VerifierId = request.VerifierId,
            VerificationTimestamp = DateTimeOffset.UtcNow,
            IsVerified = true,
            VerificationMethod = request.VerificationMethod,
            Notes = "Voter registration verified successfully."
        };

        _context.VerificationLogs.Add(successLog);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Map to unencrypted DTO response
        return new VoterRegistrationDto
        {
            Id = voter.Id,
            EpicNumber = decryptedEpic,
            FullName = decryptedName,
            Age = voter.Age,
            DateOfBirth = decryptedDob,
            Gender = voter.Gender,
            AssemblyConstituency = voter.AssemblyConstituency,
            PartNumber = voter.PartNumber,
            SectionNumber = voter.SectionNumber,
            SerialNumber = voter.SerialNumber,
            PollingStationName = voter.PollingStationName,
            PollingStationLocation = voter.PollingStationLocation,
            HouseNo = voter.HouseNo,
            BloName = voter.BloName,
            BloContact = decryptedBloContact,
            IsVerified = true
        };
    }
}
