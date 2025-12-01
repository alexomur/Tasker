using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Infrastructure;

/// <summary>
/// Контекст базы данных для записи домена досок (BoardWrite).
/// Содержит наборы сущностей и конфигурацию сопоставления с MySQL.
/// </summary>
public class BoardWriteDbContext : DbContext
{
    /// <summary>
    /// Набор досок, являющихся корневыми агрегатами.
    /// </summary>
    public DbSet<Board> Boards => Set<Board>();

    /// <summary>
    /// Набор колонок, принадлежащих доскам.
    /// </summary>
    public DbSet<Column> Columns => Set<Column>();

    /// <summary>
    /// Набор карточек, принадлежащих колонкам.
    /// </summary>
    public DbSet<Card> Cards => Set<Card>();

    /// <summary>
    /// Набор участников досок.
    /// </summary>
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();

    /// <summary>
    /// Набор меток, привязанных к доскам и карточкам.
    /// </summary>
    public DbSet<Label> Labels => Set<Label>();

    public BoardWriteDbContext(DbContextOptions<BoardWriteDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBoard(modelBuilder);
        ConfigureColumn(modelBuilder);
        ConfigureCard(modelBuilder);
        ConfigureBoardMember(modelBuilder);
        ConfigureLabel(modelBuilder);
    }

    private static void ConfigureBoard(ModelBuilder modelBuilder)
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
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.HasMany(b => b.Columns)
            .WithOne()
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Members)
            .WithOne()
            .HasForeignKey(m => m.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Board -> Label (одна доска, много меток), FK будет теневым свойством BoardId у Label
        builder.HasMany(b => b.Labels)
            .WithOne()
            .HasForeignKey("BoardId")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureColumn(ModelBuilder modelBuilder)
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
            .HasMaxLength(1000);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.HasIndex(c => new { c.BoardId, c.Order });
    }

    private static void ConfigureCard(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Card>();

        builder.ToTable("cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ColumnId)
            .IsRequired();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.Order)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedByUserId)
            .IsRequired();

        builder.Property(c => c.DueDate)
            .IsRequired(false);

        // FK: Card -> Column (многие к одному)
        builder.HasOne<Column>()
            .WithMany()
            .HasForeignKey(c => c.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // assigneeUserIds храним как отдельную таблицу (многие-ко-многим Card <-> UserId),
        // здесь пока ничего не настраиваем, т.к. сущности User в этом контексте нет.

        // Card <-> Label (многие-ко-многим) через промежуточную таблицу
        builder
            .HasMany(c => c.Labels)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "card_labels",
                right => right
                    .HasOne<Label>()
                    .WithMany()
                    .HasForeignKey("LabelId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Card>()
                    .WithMany()
                    .HasForeignKey("CardId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("CardId", "LabelId");
                });
    }

    private static void ConfigureBoardMember(ModelBuilder modelBuilder)
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
            .IsRequired();

        builder.Property(m => m.LeftAt)
            .IsRequired(false);

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
            .HasMaxLength(20);
    }
}
