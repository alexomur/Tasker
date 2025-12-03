using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Infrastructure;

public sealed class BoardWriteDbContext : DbContext
{
    public BoardWriteDbContext(DbContextOptions<BoardWriteDbContext> options) : base(options)
    {
    }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<Label> Labels => Set<Label>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet("utf8mb4").UseCollation("utf8mb4_0900_ai_ci");

        var dtoToUtc = new ValueConverter<DateTimeOffset, DateTime>(
            v => v.UtcDateTime,
            v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc), TimeSpan.Zero)
        );

        ConfigureBoard(modelBuilder, dtoToUtc);
        ConfigureColumn(modelBuilder, dtoToUtc);
        ConfigureCard(modelBuilder, dtoToUtc);
        ConfigureBoardMember(modelBuilder, dtoToUtc);
        ConfigureLabel(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureBoard(ModelBuilder modelBuilder, ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
    {
        var builder = modelBuilder.Entity<Board>();

        builder.ToTable("boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.OwnerUserId)
            .IsRequired();

        builder.Property(b => b.IsArchived)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.HasMany(b => b.Columns)
            .WithOne()
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Members)
            .WithOne()
            .HasForeignKey(m => m.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Labels)
            .WithOne()
            .HasForeignKey("BoardId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Cards)
            .WithOne()
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.OwnerUserId);

        // доменные события базе не нужны
        builder.Ignore(b => b.DomainEvents);
    }

    private static void ConfigureColumn(ModelBuilder modelBuilder, ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
    {
        var builder = modelBuilder.Entity<Column>();

        builder.ToTable("columns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BoardId)
            .IsRequired();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.HasIndex(c => c.BoardId);
        builder.HasIndex(c => new { c.BoardId, c.Order });

        builder.Ignore(c => c.DomainEvents);
    }

    private static void ConfigureCard(ModelBuilder modelBuilder, ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
    {
        var builder = modelBuilder.Entity<Card>();

        builder.ToTable("cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BoardId)
            .IsRequired();

        builder.Property(c => c.ColumnId)
            .IsRequired();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.Property(c => c.CreatedByUserId)
            .IsRequired();

        builder.Property(c => c.DueDate)
            .HasConversion(dtoToUtc);

        builder.Ignore(c => c.Labels);
        builder.Ignore(c => c.AssigneeUserIds);
        builder.Ignore(c => c.DomainEvents);

        builder.HasIndex(c => c.BoardId);
        builder.HasIndex(c => new { c.BoardId, c.ColumnId, c.Order });
    }

    private static void ConfigureBoardMember(ModelBuilder modelBuilder, ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
    {
        var builder = modelBuilder.Entity<BoardMember>();

        builder.ToTable("board_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.BoardId)
            .IsRequired();

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired();

        builder.Property(m => m.JoinedAt)
            .HasConversion(dtoToUtc)
            .IsRequired();

        builder.Property(m => m.LeftAt)
            .HasConversion(dtoToUtc);

        builder.Ignore(m => m.IsActive);
        builder.Ignore(m => m.DomainEvents);

        builder.HasIndex(m => m.BoardId);
        builder.HasIndex(m => new { m.BoardId, m.UserId });
    }

    private static void ConfigureLabel(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Label>();

        builder.ToTable("labels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(50);

        // FK к доске — через теневое свойство
        builder.Property<Guid>("BoardId")
            .IsRequired();

        builder.HasIndex("BoardId");

        builder.Ignore(l => l.DomainEvents);
    }
}
