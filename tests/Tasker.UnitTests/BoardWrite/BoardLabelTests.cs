using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.UnitTests.BoardWrite;

public sealed class BoardLabelTests
{
    [Fact]
    public void AttachLabelToCard_AddsLabelToCard()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);

        var label = board.AddLabel("Bug", "#ff0000", ownerId, now);
        var column = board.AddColumn("Todo", ownerId, now);
        var card = board.CreateCard(column.Id, "Task 1", ownerId, now);

        var later = now.AddMinutes(5);
        board.AttachLabelToCard(card.Id, label.Id, ownerId, later);

        Assert.Single(card.Labels);
        Assert.Contains(label, card.Labels);
        Assert.Equal(later, board.UpdatedAt);
    }

    [Fact]
    public void AttachLabelToCard_Twice_DoesNotCreateDuplicates()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);

        var label = board.AddLabel("Bug", "#ff0000", ownerId, now);
        var column = board.AddColumn("Todo", ownerId, now);
        var card = board.CreateCard(column.Id, "Task 1", ownerId, now);

        var t1 = now.AddMinutes(1);
        var t2 = now.AddMinutes(2);

        board.AttachLabelToCard(card.Id, label.Id, ownerId, t1);
        board.AttachLabelToCard(card.Id, label.Id, ownerId, t2);

        Assert.Single(card.Labels);
        Assert.Contains(label, card.Labels);
    }

    [Fact]
    public void AttachLabelToCard_UnknownCard_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);
        var label = board.AddLabel("Bug", "#ff0000", ownerId, now);

        var cardId = Guid.NewGuid();

        var ex = Assert.Throws<InvalidOperationException>(
            () => board.AttachLabelToCard(cardId, label.Id, ownerId, now.AddMinutes(1)));

        Assert.Contains("Карточка не найдена", ex.Message);
    }

    [Fact]
    public void AttachLabelToCard_UnknownLabel_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);

        var column = board.AddColumn("Todo", ownerId, now);
        var card = board.CreateCard(column.Id, "Task 1", ownerId, now);
        var labelId = Guid.NewGuid();

        var ex = Assert.Throws<InvalidOperationException>(
            () => board.AttachLabelToCard(card.Id, labelId, ownerId, now.AddMinutes(1)));

        Assert.Contains("Метка не найдена", ex.Message);
    }

    [Fact]
    public void DetachLabelFromCard_RemovesLabel()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);

        var label = board.AddLabel("Bug", "#ff0000", ownerId, now);
        var column = board.AddColumn("Todo", ownerId, now);
        var card = board.CreateCard(column.Id, "Task 1", ownerId, now);

        var t1 = now.AddMinutes(1);
        var t2 = now.AddMinutes(2);

        board.AttachLabelToCard(card.Id, label.Id, ownerId, t1);
        board.DetachLabelFromCard(card.Id, label.Id, ownerId, t2);

        Assert.Empty(card.Labels);
        Assert.Equal(t2, board.UpdatedAt);
    }

    [Fact]
    public void DetachLabelFromCard_WhenLabelNotOnCard_DoesNothing()
    {
        var now = DateTimeOffset.UtcNow;
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Test board", ownerId, now);

        var label = board.AddLabel("Bug", "#ff0000", ownerId, now);
        var column = board.AddColumn("Todo", ownerId, now);
        var card = board.CreateCard(column.Id, "Task 1", ownerId, now);

        var beforeUpdated = board.UpdatedAt;
        var later = now.AddMinutes(5);

        board.DetachLabelFromCard(card.Id, label.Id, ownerId, later);

        Assert.Empty(card.Labels);
        Assert.Equal(later, board.UpdatedAt);
    }
}


