using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasker.Core.Boards;

namespace Tasker.Data.Configurations;

internal class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.ToTable("Columns");
        builder.HasKey(c => c.Id);

        builder.Property<string>("Title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property<string?>("Description")
            .IsRequired(false);
        
        builder.HasMany(typeof(Card), "_cards")
            .WithOne()
            .HasForeignKey("ColumnId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Column.Cards))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex("BoardId");
    }
}