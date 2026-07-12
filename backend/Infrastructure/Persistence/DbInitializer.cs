using System;
using System.Linq;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Domain.Entities;

namespace MatdanSathi.API.Infrastructure.Persistence;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context, ICryptographyService cryptographyService)
    {
        // 1. Ensure migrations are applied (handled in Program.cs, but context is ready)
        context.Database.EnsureCreated();

        // 2. Seed BLO Coordinates for geolocated proximity search
        if (!context.BloCoordinateMaps.Any())
        {
            var bloMaps = new[]
            {
                new BloCoordinateMap
                {
                    BloName = "Ahmed Khan",
                    BloContactEncrypted = cryptographyService.Encrypt("1111122222"),
                    PollingStationName = "Primary School Building East",
                    Latitude = 18.5204,
                    Longitude = 73.8567,
                    BoundaryCoordinatesJson = "[[73.85, 18.52], [73.86, 18.52], [73.86, 18.53], [73.85, 18.53], [73.85, 18.52]]"
                },
                new BloCoordinateMap
                {
                    BloName = "Yasmin Shaikh",
                    BloContactEncrypted = cryptographyService.Encrypt("2222233333"),
                    PollingStationName = "BMC School Hall West",
                    Latitude = 19.0760,
                    Longitude = 72.8777,
                    BoundaryCoordinatesJson = "[[72.87, 19.07], [72.88, 19.07], [72.88, 19.08], [72.87, 19.08], [72.87, 19.07]]"
                },
                new BloCoordinateMap
                {
                    BloName = "Ziaul Haq",
                    BloContactEncrypted = cryptographyService.Encrypt("3333344444"),
                    PollingStationName = "Government Primary School Room 1",
                    Latitude = 18.5312,
                    Longitude = 73.8445,
                    BoundaryCoordinatesJson = "[[73.84, 18.53], [73.85, 18.53], [73.85, 18.54], [73.84, 18.54], [73.84, 18.53]]"
                }
            };

            context.BloCoordinateMaps.AddRange(bloMaps);
            context.SaveChanges();
        }

        // 3. Seed Voter Profiles representing the Baseline Electoral Roll
        if (context.VoterProfiles.Any())
        {
            var testBlindIndex = cryptographyService.GenerateBlindIndex("SLD1234567");
            var oldGrandmother = context.VoterProfiles.FirstOrDefault(v => v.EpicNumberBlindIndex == testBlindIndex);
            if (oldGrandmother != null)
            {
                var decryptedName = cryptographyService.Decrypt(oldGrandmother.FullNameEncrypted);
                if (decryptedName != "Khan Saidnabi" || context.VoterProfiles.Count() < 5)
                {
                    context.VoterProfiles.RemoveRange(context.VoterProfiles);
                    context.SaveChanges();
                }
            }
        }

        if (!context.VoterProfiles.Any())
        {
            var voterProfiles = new[]
            {
                // grandmother's mock baseline profile
                new VoterProfile
                {
                    EpicNumber = "SLD1234567",
                    FullName = "Khan Saidnabi",
                    Age = 78,
                    Gender = "Female",
                    DateOfBirth = "05/06/1948",
                    AssemblyConstituency = "Constituency-1",
                    PartNumber = "Part-1",
                    SectionNumber = "Section-1",
                    SerialNumber = 12,
                    PollingStationName = "Primary School Building East",
                    PollingStationLocation = "Room No 2",
                    HouseNo = "42-A/1",
                    BloName = "Ahmed Khan",
                    BloContact = "1111122222"
                },
                new VoterProfile
                {
                    EpicNumber = "SLD9876543",
                    FullName = "Ramesh Sawant",
                    Age = 45,
                    Gender = "Male",
                    DateOfBirth = "12/12/1980",
                    AssemblyConstituency = "Constituency-1",
                    PartNumber = "Part-1",
                    SectionNumber = "Section-1",
                    SerialNumber = 13,
                    PollingStationName = "Primary School Building East",
                    PollingStationLocation = "Room No 2",
                    HouseNo = "42-A/2",
                    BloName = "Ahmed Khan",
                    BloContact = "1111122222"
                },
                new VoterProfile
                {
                    EpicNumber = "SLD2345678",
                    FullName = "Deepa Joshi",
                    Age = 29,
                    Gender = "Female",
                    DateOfBirth = "10/10/1996",
                    AssemblyConstituency = "Constituency-1",
                    PartNumber = "Part-1",
                    SectionNumber = "Section-1",
                    SerialNumber = 14,
                    PollingStationName = "Primary School Building East",
                    PollingStationLocation = "Room No 2",
                    HouseNo = "43",
                    BloName = "Ahmed Khan",
                    BloContact = "1111122222"
                },
                new VoterProfile
                {
                    EpicNumber = "SLD5544332",
                    FullName = "Imran Shaikh",
                    Age = 42,
                    Gender = "Male",
                    DateOfBirth = "20/04/1983",
                    AssemblyConstituency = "Constituency-1",
                    PartNumber = "Part-1",
                    SectionNumber = "Section-1",
                    SerialNumber = 15,
                    PollingStationName = "Primary School Building East",
                    PollingStationLocation = "Room No 2",
                    HouseNo = "45/A",
                    BloName = "Ahmed Khan",
                    BloContact = "1111122222"
                },
                new VoterProfile
                {
                    EpicNumber = "SLD6677889",
                    FullName = "Farida Begum",
                    Age = 38,
                    Gender = "Female",
                    DateOfBirth = "15/08/1987",
                    AssemblyConstituency = "Constituency-1",
                    PartNumber = "Part-1",
                    SectionNumber = "Section-1",
                    SerialNumber = 16,
                    PollingStationName = "Primary School Building East",
                    PollingStationLocation = "Room No 2",
                    HouseNo = "48",
                    BloName = "Ahmed Khan",
                    BloContact = "1111122222"
                }
            };

            context.VoterProfiles.AddRange(voterProfiles);
            context.SaveChanges();
        }
    }
}
