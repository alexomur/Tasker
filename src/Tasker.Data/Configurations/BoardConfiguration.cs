using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasker.Core.Boards;

namespace Tasker.Data.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");
        builder.HasKey(b => b.Id);
        
        builder.Property<string>("Title")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property<string?>("Description")
            .IsRequired(false);

        builder.HasMany(typeof(Column), "_columns")
            .WithOne()
            .HasForeignKey("BoardId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Metadata.FindNavigation(nameof(Column))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}