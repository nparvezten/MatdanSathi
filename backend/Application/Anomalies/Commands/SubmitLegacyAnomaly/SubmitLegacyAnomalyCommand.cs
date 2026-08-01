using System;
using System.Collections.Generic;
using MatdarSathi.API.Application.Common.Interfaces;

namespace MatdarSathi.API.Application.Anomalies.Commands.SubmitLegacyAnomaly;

public record FamilyMemberDto(string MemberName, string Relation, int Age, string? EpicNumber);

public record SubmitLegacyAnomalyCommand(
    string ReceiptNumber,
    string DeceasedName,
    int YearOfDeath,
    string DeathCertRegNo,
    string ConstituencyName,
    string PartNumber,
    string PageNumber,
    string SerialRange,
    List<FamilyMemberDto> FamilyMembers
) : IRequest<LegacyAnomalyResponseDto>;

public record LegacyAnomalyResponseDto(
    Guid Id,
    string ReceiptNumber,
    string Status,
    string Message,
    DateTime CreatedAt
);
