using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Domain.Common;
using MatdanSathi.API.Domain.Entities;

namespace MatdanSathi.API.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICryptographyService _cryptographyService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICryptographyService cryptographyService) : base(options)
    {
        _cryptographyService = cryptographyService;
    }

    public DbSet<VoterProfile> VoterProfiles => Set<VoterProfile>();
    public DbSet<VerificationLog> VerificationLogs => Set<VerificationLog>();
    public DbSet<BloCoordinateMap> BloCoordinateMaps => Set<BloCoordinateMap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- VoterProfile Configuration ---
        modelBuilder.Entity<VoterProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Deterministic Blind Index for exact match must be indexed
            entity.HasIndex(e => e.EpicNumberBlindIndex)
                .IsUnique();

            // Setup soft-delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Optimistic concurrency control using RowVersion
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken();
        });

        // --- VerificationLog Configuration ---
        modelBuilder.Entity<VerificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken();

            entity.HasOne(e => e.VoterProfile)
                .WithMany()
                .HasForeignKey(e => e.VoterProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- BloCoordinateMap Configuration ---
        modelBuilder.Entity<BloCoordinateMap>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken();
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Process unmapped cleartext properties for VoterProfiles (encryption + blind indexing)
        foreach (var entry in ChangeTracker.Entries<VoterProfile>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (!string.IsNullOrEmpty(entry.Entity.EpicNumber))
                {
                    entry.Entity.EpicNumberBlindIndex = _cryptographyService.GenerateBlindIndex(entry.Entity.EpicNumber.Trim().ToUpperInvariant());
                    entry.Entity.EpicNumberEncrypted = _cryptographyService.Encrypt(entry.Entity.EpicNumber.Trim().ToUpperInvariant());
                }
                if (!string.IsNullOrEmpty(entry.Entity.FullName))
                {
                    entry.Entity.FullNameEncrypted = _cryptographyService.Encrypt(entry.Entity.FullName.Trim());
                }
                if (!string.IsNullOrEmpty(entry.Entity.DateOfBirth))
                {
                    entry.Entity.DateOfBirthEncrypted = _cryptographyService.Encrypt(entry.Entity.DateOfBirth.Trim());
                }
                if (!string.IsNullOrEmpty(entry.Entity.BloContact))
                {
                    entry.Entity.BloContactEncrypted = _cryptographyService.Encrypt(entry.Entity.BloContact.Trim());
                }
            }
        }

        // 2. Automatically handle audit stamps, soft deletes, and concurrency token rotation
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray(); // Rotate row version
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray(); // Rotate row version
                    break;

                case EntityState.Deleted:
                    // Soft-delete interceptor
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray(); // Rotate row version
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
