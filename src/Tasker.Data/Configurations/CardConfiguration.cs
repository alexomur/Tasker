using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasker.Core.Boards;

namespace Tasker.Data.Configurations;

internal class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");
        builder.HasKey(c => c.Id);

        builder.Property<string>("Title")
            .IsRequired()
            .HasMaxLength(400);

        builder.Property<string?>("Description")
            .IsRequired(false);

        builder.HasIndex("ColumnId");
    }
}