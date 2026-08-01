using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MatdarSathi.API.Application.Common.Interfaces;

public record ParsedVoterRecord(
    string EpicNumber,
    string FullName,
    int Age,
    string Gender,
    string HouseNo,
    string AssemblyConstituency,
    string PartNumber,
    string SectionNumber,
    string SerialNumber,
    string PollingStationName,
    string PollingStationLocation
);

public interface IWatchdogComparisonService
{
    Task<int> CompareAndIngestParsedRecordsAsync(
        string boothId,
        List<ParsedVoterRecord> records,
        CancellationToken cancellationToken = default);
}
