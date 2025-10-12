using Microsoft.EntityFrameworkCore;
using Tasker.Core.Boards;

namespace Tasker.Data;

public class TaskerDbContext : DbContext
{
    public TaskerDbContext(DbContextOptions<TaskerDbContext> options) : base(options) { }
    
    public DbSet<Board> Boards => Set<Board>();
    
    public DbSet<Column> Columns => Set<Column>();
    
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var board = modelBuilder.Entity<Board>();
        board.ToTable("Boards");
        board.HasKey(b => b.Id);
        board.Property(b => b.Title).IsRequired();
        board.HasMany<Column>("_columns")
            .WithOne()
            .HasForeignKey("BoardId")
            .OnDelete(DeleteBehavior.Cascade);
        board.Navigation(b => b.Columns).UsePropertyAccessMode(PropertyAccessMode.Field);

        var column = modelBuilder.Entity<Column>();
        column.ToTable("Columns");
        column.HasKey(c => c.Id);
        column.Property(c => c.Title).IsRequired();
        column.Property<Guid>("BoardId");
        column.Property<int>("OrderIndex");
        column.HasIndex("BoardId");
        column.HasIndex("BoardId", "OrderIndex");
        column.HasMany<Card>("_cards")
            .WithOne()
            .HasForeignKey("ColumnId")
            .OnDelete(DeleteBehavior.Cascade);
        column.Navigation(c => c.Cards).UsePropertyAccessMode(PropertyAccessMode.Field);

        var card = modelBuilder.Entity<Card>();
        card.ToTable("Cards");
        card.HasKey(c => c.Id);
        card.Property(c => c.Title).IsRequired();
        card.Property<Guid>("ColumnId");
        card.Property<int>("OrderIndex");
        card.HasIndex("ColumnId");
        card.HasIndex("ColumnId", "OrderIndex");
    }

    public override int SaveChanges()
    {
        ApplyOrderingIndices();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyOrderingIndices();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyOrderingIndices()
    {
        foreach (var boardEntry in ChangeTracker.Entries<Board>()
                     .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            var board = boardEntry.Entity;
            var columns = board.Columns.ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                Entry(columns[i]).Property("OrderIndex").CurrentValue = i;
            }

            foreach (List<Card>? cards in columns.Select(column => column.Cards.ToList()))
            {
                for (int j = 0; j < cards.Count; j++)
                {
                    Entry(cards[j]).Property("OrderIndex").CurrentValue = j;
                }
            }
        }

        foreach (var columnEntry in ChangeTracker.Entries<Column>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var column = columnEntry.Entity;
            var cards = column.Cards.ToList();
            for (int j = 0; j < cards.Count; j++)
            {
                Entry(cards[j]).Property("OrderIndex").CurrentValue = j;
            }
        }
    }
}
