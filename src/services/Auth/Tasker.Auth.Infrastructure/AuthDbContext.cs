using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tasker.Auth.Domain.Users;
using Tasker.Auth.Domain.ValueObjects;
using Tasker.Auth.Infrastructure.Configurations;

namespace Tasker.Auth.Infrastructure;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder )
    {
        modelBuilder.HasCharSet("utf8mb4").UseCollation("utf8mb4_0900_ai_ci");
        
        var emailConverter = new ValueConverter<EmailAddress, string>(
            v => v.Value,
            v => EmailAddress.Create(v)
        );
        
        var dtoToUtc = new ValueConverter<DateTimeOffset, DateTime>(
            v => v.UtcDateTime,
            v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc), TimeSpan.Zero)
        );

        modelBuilder.ApplyConfiguration(new UserConfiguration(emailConverter, dtoToUtc));
        base.OnModelCreating(modelBuilder);
    }
}