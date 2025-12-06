using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.UnitTests.BoardWrite;

public sealed class CardLabelTests
{
    [Fact]
    public void AddLabel_FirstTime_AddsLabel()
    {
        var now = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, now);
        var label = new Label("Bug", null, "#ff0000");

        card.AddLabel(label, now);

        Assert.Single(card.Labels);
        Assert.Contains(label, card.Labels);
    }

    [Fact]
    public void AddLabel_SecondTime_DoesNotDuplicate()
    {
        var now = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, now);
        var label = new Label("Bug", null, "#ff0000");

        card.AddLabel(label, now);
        card.AddLabel(label, now.AddMinutes(1));

        Assert.Single(card.Labels);
        Assert.Contains(label, card.Labels);
    }

    [Fact]
    public void RemoveLabel_WhenExists_RemovesLabel()
    {
        var now = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, now);
        var label = new Label("Bug", null, "#ff0000");

        card.AddLabel(label, now);
        card.RemoveLabel(label.Id, now.AddMinutes(1));

        Assert.Empty(card.Labels);
    }

    [Fact]
    public void RemoveLabel_WhenNotExists_DoesNothing()
    {
        var now = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, now);

        card.RemoveLabel(Guid.NewGuid(), now.AddMinutes(1));

        Assert.Empty(card.Labels);
    }

    [Fact]
    public void AddLabel_UpdatesUpdatedAt_AndKeepsCreatedAt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, createdAt);
        var label = new Label("Bug", null, "#ff0000");
        var later = createdAt.AddMinutes(5);

        card.AddLabel(label, later);

        Assert.Equal(createdAt, card.CreatedAt);
        Assert.Equal(later, card.UpdatedAt);
    }

    [Fact]
    public void RemoveLabel_UpdatesUpdatedAt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var card = Card.Create(Guid.NewGuid(), Guid.NewGuid(), "Test card", Guid.NewGuid(), 1, createdAt);
        var label = new Label("Bug", null, "#ff0000");
        var addedAt = createdAt.AddMinutes(1);
        var removedAt = createdAt.AddMinutes(5);

        card.AddLabel(label, addedAt);
        card.RemoveLabel(label.Id, removedAt);

        Assert.Equal(removedAt, card.UpdatedAt);
    }
}
