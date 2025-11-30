using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tasker.Auth.Domain.Users;

namespace Tasker.Auth.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private readonly ValueConverter _emailConverter;
    
    private readonly ValueConverter _dtoToUtc;

    public UserConfiguration(ValueConverter emailConverter, ValueConverter dtoToUtc)
    {
        _emailConverter = emailConverter;
        _dtoToUtc = dtoToUtc;
    }

    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Email)
            .HasConversion(_emailConverter)
            .HasMaxLength(320)
            .IsRequired();

        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.DisplayName)
            .HasMaxLength(64)
            .IsRequired();

        b.Property(x => x.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.EmailConfirmed)
            .HasColumnType("tinyint(1)")
            .IsRequired();

        b.Property(x => x.IsLocked)
            .HasColumnType("tinyint(1)")
            .IsRequired();

        // даты — в datetime(6) UTC
        b.Property(x => x.LockedAt)
            .HasConversion(_dtoToUtc)
            .HasColumnType("datetime(6)");

        b.Property(x => x.CreatedAt)
            .HasConversion(_dtoToUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasConversion(_dtoToUtc)
            .HasColumnType("datetime(6)")
            .IsRequired();

        b.Property(x => x.LastPasswordChangedAt)
            .HasConversion(_dtoToUtc)
            .HasColumnType("datetime(6)");

        b.Property(x => x.LockReason)
            .HasMaxLength(256);
        
        b.HasIndex(x => x.DisplayName);
    }
}