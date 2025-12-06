using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
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
            v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc), TimeSpan.Zero));

        var assigneeIdsConverter = new ValueConverter<List<Guid>, string>(
            v => SerializeAssigneeIds(v),
            v => DeserializeAssigneeIds(v));

        var assigneeIdsComparer = new ValueComparer<List<Guid>>(
            (l1, l2) =>
                l1 == l2 ||
                (l1 != null && l2 != null && l1.SequenceEqual(l2)),
            l =>
                l.Aggregate(0, (acc, guid) => acc ^ guid.GetHashCode()),
            l =>
                l.ToList());

        ConfigureBoard(modelBuilder, dtoToUtc);
        ConfigureColumn(modelBuilder, dtoToUtc);
        ConfigureCard(modelBuilder, dtoToUtc, assigneeIdsConverter, assigneeIdsComparer);
        ConfigureBoardMember(modelBuilder, dtoToUtc);
        ConfigureLabel(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static string SerializeAssigneeIds(List<Guid> ids) =>
        JsonSerializer.Serialize(ids);

    private static List<Guid> DeserializeAssigneeIds(string json) =>
        string.IsNullOrEmpty(json)
            ? new List<Guid>()
            : JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();

    private static void ConfigureBoard(
        ModelBuilder modelBuilder,
        ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
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

        builder.Ignore(b => b.DomainEvents);
    }

    private static void ConfigureColumn(
        ModelBuilder modelBuilder,
        ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
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

    private static void ConfigureCard(
        ModelBuilder modelBuilder,
        ValueConverter<DateTimeOffset, DateTime> dtoToUtc,
        ValueConverter<List<Guid>, string> assigneeIdsConverter,
        ValueComparer<List<Guid>> assigneeIdsComparer)
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

        builder.Property<List<Guid>>("_assigneeUserIds")
            .HasColumnName("assignee_user_ids")
            .HasConversion(assigneeIdsConverter)
            .Metadata.SetValueComparer(assigneeIdsComparer);

        // many-to-many Card <-> Label через join-таблицу card_labels
        builder.HasMany(c => c.Labels)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "card_labels",
                j => j
                    .HasOne<Label>()
                    .WithMany()
                    .HasForeignKey("LabelId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Card>()
                    .WithMany()
                    .HasForeignKey("CardId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("card_labels");
                    j.HasKey("CardId", "LabelId");
                    j.HasIndex("LabelId");
                });

        builder.Navigation(c => c.Labels)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(c => c.AssigneeUserIds);
        builder.Ignore(c => c.DomainEvents);

        builder.HasIndex(c => c.BoardId);
        builder.HasIndex(c => new { c.BoardId, c.ColumnId, c.Order });
    }

    private static void ConfigureBoardMember(
        ModelBuilder modelBuilder,
        ValueConverter<DateTimeOffset, DateTime> dtoToUtc)
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

        builder.Property<Guid>("BoardId")
            .IsRequired();

        builder.HasIndex("BoardId");

        builder.Ignore(l => l.DomainEvents);
    }
}
