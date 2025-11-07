using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Tasker.Auth.Infrastructure;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = cfg.GetConnectionString("Auth")
                   ?? "Server=127.0.0.1;Port=3306;Database=tasker;User=tasker;Password=dev;TreatTinyAsBoolean=true;";

        // ВАЖНО: фиксируем версию сервера, НЕ AutoDetect (чтобы не коннектиться на дизайн-этапе)
        var server = new MySqlServerVersion(new Version(8, 0, 36));

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseMySql(conn, server)
            .Options;

        return new AuthDbContext(options);
    }
}