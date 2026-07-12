using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatdanSathi.API.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace MatdanSathi.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("strict-limit")]
public class BloController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptographyService;

    public BloController(IApplicationDbContext context, ICryptographyService cryptographyService)
    {
        _context = context;
        _cryptographyService = cryptographyService;
    }

    [HttpGet("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BloLookupResultDto>))]
    public async Task<IActionResult> LookupBlo([FromQuery] double latitude, [FromQuery] double longitude)
    {
        // 1. Fetch maps from database
        var maps = await _context.BloCoordinateMaps
            .Where(m => !m.IsDeleted)
            .ToListAsync();

        // 2. Perform distance sorting in memory (Haversine formula approximation)
        var results = maps.Select(map =>
        {
            double distance = CalculateDistance(latitude, longitude, map.Latitude, map.Longitude);

            // Decrypt contact number
            string contact = "N/A";
            try
            {
                contact = _cryptographyService.Decrypt(map.BloContactEncrypted);
            }
            catch
            {
                // Fallback if decryption fails (e.g., seeding data issues)
                contact = "Decryption Error";
            }

            // Crowd-sourced metrics (mocked for demo purposes, can be extended in DB schemas)
            double verificationScore = 0.92 + (new Random(map.Id.GetHashCode()).NextDouble() * 0.07); // 92% - 99%
            int verificationCount = 12 + new Random(map.Id.GetHashCode()).Next(0, 150);

            return new BloLookupResultDto
            {
                Id = map.Id,
                BloName = map.BloName,
                BloContact = contact,
                PollingStationName = map.PollingStationName,
                Latitude = map.Latitude,
                Longitude = map.Longitude,
                DistanceInKm = Math.Round(distance, 2),
                VerificationScore = Math.Round(verificationScore * 100, 1),
                VerificationCount = verificationCount
            };
        })
        .OrderBy(r => r.DistanceInKm)
        .Take(5) // Return 5 closest locations
        .ToList();

        // If no records in database, return a mock listing for testing/demo
        if (results.Count == 0)
        {
            results = GetMockBloListings(latitude, longitude);
        }

        return Ok(results);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double val) => (Math.PI / 180) * val;

    private List<BloLookupResultDto> GetMockBloListings(double lat, double lon)
    {
        return new List<BloLookupResultDto>
        {
            new() {
                Id = Guid.NewGuid(),
                BloName = "Rajesh Sharma",
                BloContact = "+91 98765 43210",
                PollingStationName = "Primary School Sector-4 (Room 1)",
                Latitude = lat + 0.003,
                Longitude = lon - 0.002,
                DistanceInKm = 0.45,
                VerificationScore = 97.4,
                VerificationCount = 132
            },
            new() {
                Id = Guid.NewGuid(),
                BloName = "Anjali Deshmukh",
                BloContact = "+91 91234 56789",
                PollingStationName = "Government Girls High School (West Wing)",
                Latitude = lat - 0.006,
                Longitude = lon + 0.005,
                DistanceInKm = 0.82,
                VerificationScore = 95.8,
                VerificationCount = 98
            },
            new() {
                Id = Guid.NewGuid(),
                BloName = "Vikram Singh",
                BloContact = "+91 88888 77777",
                PollingStationName = "Community Hall Ward 12",
                Latitude = lat + 0.012,
                Longitude = lon + 0.009,
                DistanceInKm = 1.61,
                VerificationScore = 92.1,
                VerificationCount = 47
            }
        };
    }
}

public record BloLookupResultDto
{
    public Guid Id { get; init; }
    public string BloName { get; init; } = null!;
    public string BloContact { get; init; } = null!;
    public string PollingStationName { get; init; } = null!;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double DistanceInKm { get; init; }
    public double VerificationScore { get; init; }
    public int VerificationCount { get; init; }
}
