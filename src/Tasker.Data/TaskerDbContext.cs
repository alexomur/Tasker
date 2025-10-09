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
        
    }
}