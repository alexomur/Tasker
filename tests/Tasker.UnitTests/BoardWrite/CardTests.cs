using FluentAssertions;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Domain.Events.CardEvents;

namespace Tasker.UnitTests.BoardWrite;

public class CardTests
{
    [Fact]
    public void Create_ShouldInitializeProperties()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        // Act
        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "  Test card  ",
            createdByUserId: createdByUserId,
            order: 5,
            now: now,
            description: "  Description ",
            dueDate: now.AddDays(1));

        // Assert
        card.Id.Should().NotBe(Guid.Empty);
        card.BoardId.Should().Be(boardId);
        card.ColumnId.Should().Be(columnId);
        card.Title.Should().Be("Test card");
        card.Description.Should().Be("Description");
        card.CreatedByUserId.Should().Be(createdByUserId);
        card.Order.Should().Be(5);
        card.CreatedAt.Should().Be(now);
        card.UpdatedAt.Should().Be(now);
        card.DueDate.Should().Be(now.AddDays(1));
        card.AssigneeUserIds.Should().BeEmpty();
        card.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetDueDate_ShouldUpdateDueDateAndRaiseEvent()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "Card",
            createdByUserId: createdByUserId,
            order: 1,
            now: now);

        var newDueDate = now.AddDays(3);
        var eventTime = now.AddMinutes(10);

        // Act
        card.SetDueDate(newDueDate, eventTime);

        // Assert
        card.DueDate.Should().Be(newDueDate);
        card.UpdatedAt.Should().Be(eventTime);

        var evt = card.DomainEvents
            .OfType<CardDueDateChanged>()
            .Single();

        evt.BoardId.Should().Be(boardId);
        evt.CardId.Should().Be(card.Id);
        evt.NewDueDate.Should().Be(newDueDate);
        evt.OccurredAt.Should().Be(eventTime);
    }

    [Fact]
    public void AssignUser_ShouldAddAssigneeAndRaiseEvent()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();

        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "Card",
            createdByUserId: createdByUserId,
            order: 1,
            now: now);

        var eventTime = now.AddMinutes(5);

        // Act
        card.AssignUser(assigneeId, eventTime);

        // Assert
        card.AssigneeUserIds.Should().ContainSingle()
            .Which.Should().Be(assigneeId);
        card.UpdatedAt.Should().Be(eventTime);

        var evt = card.DomainEvents
            .OfType<CardAssigneesChanged>()
            .Single();

        evt.BoardId.Should().Be(boardId);
        evt.CardId.Should().Be(card.Id);
        evt.AssigneeUserIds.Should().ContainSingle()
            .Which.Should().Be(assigneeId);
        evt.OccurredAt.Should().Be(eventTime);
    }

    [Fact]
    public void AssignUser_WhenAlreadyAssigned_ShouldNotDuplicateAssigneeOrRaiseNewEvent()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();

        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "Card",
            createdByUserId: createdByUserId,
            order: 1,
            now: now);

        var eventTime = now.AddMinutes(5);

        card.AssignUser(assigneeId, eventTime);
        var eventsAfterFirstAssign = card.DomainEvents.Count;

        // Act
        card.AssignUser(assigneeId, eventTime.AddMinutes(1));

        // Assert
        card.AssigneeUserIds.Should().ContainSingle()
            .Which.Should().Be(assigneeId);
        card.DomainEvents.Should().HaveCount(eventsAfterFirstAssign);
    }

    [Fact]
    public void UnassignUser_WhenExisting_ShouldRemoveAssigneeAndRaiseEvent()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();

        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "Card",
            createdByUserId: createdByUserId,
            order: 1,
            now: now);

        card.AssignUser(assigneeId, now.AddMinutes(1));
        card.ClearDomainEvents(); // чистим события от AssignUser, чтобы проверять только Unassign

        var eventTime = now.AddMinutes(2);

        // Act
        card.UnassignUser(assigneeId, eventTime);

        // Assert
        card.AssigneeUserIds.Should().BeEmpty();
        card.UpdatedAt.Should().Be(eventTime);

        var evt = card.DomainEvents
            .OfType<CardAssigneesChanged>()
            .Single();

        evt.AssigneeUserIds.Should().BeEmpty();
        evt.BoardId.Should().Be(boardId);
        evt.CardId.Should().Be(card.Id);
        evt.OccurredAt.Should().Be(eventTime);
    }

    [Fact]
    public void UnassignUser_WhenNotAssigned_ShouldDoNothing()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var notAssigneeId = Guid.NewGuid();

        var card = Card.Create(
            boardId: boardId,
            columnId: columnId,
            title: "Card",
            createdByUserId: createdByUserId,
            order: 1,
            now: now);

        // Act
        card.UnassignUser(notAssigneeId, now.AddMinutes(1));

        // Assert
        card.AssigneeUserIds.Should().BeEmpty();
        card.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rename_WithEmptyTitle_ShouldThrow()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var card = Card.Create(
            boardId: Guid.NewGuid(),
            columnId: Guid.NewGuid(),
            title: "Card",
            createdByUserId: Guid.NewGuid(),
            order: 1,
            now: now);

        // Act
        var act = () => card.Rename("   ", now.AddMinutes(1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
