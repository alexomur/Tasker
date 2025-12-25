using FluentAssertions;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.UnitTests.BoardWrite;

public class BoardTests
{
    [Fact]
    public void Create_ShouldInitializeBoardAndOwnerMember()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();

        // Act
        var board = Board.Create(
            title: "  My board  ",
            ownerUserId: ownerId,
            now: now,
            description: "  Description  ");

        // Assert
        board.Id.Should().NotBe(Guid.Empty);
        board.Title.Should().Be("My board");
        board.Description.Should().Be("Description");
        board.OwnerUserId.Should().Be(ownerId);
        board.IsArchived.Should().BeFalse();
        board.CreatedAt.Should().Be(now);
        board.UpdatedAt.Should().Be(now);

        var ownerMember = board.Members.Should().ContainSingle().Subject;
        ownerMember.BoardId.Should().Be(board.Id);
        ownerMember.UserId.Should().Be(ownerId);
        ownerMember.Role.Should().Be(BoardMemberRole.Owner);
        ownerMember.IsActive.Should().BeTrue();
        ownerMember.JoinedAt.Should().Be(now);
        ownerMember.LeftAt.Should().BeNull();
    }

    [Fact]
    public void AddMember_ShouldAddNewActiveMember()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);

        var addTime = now.AddMinutes(5);

        // Act
        var member = board.AddMember(otherUserId, BoardMemberRole.Member, ownerId, addTime);

        // Assert
        board.Members.Should().HaveCount(2);
        member.BoardId.Should().Be(board.Id);
        member.UserId.Should().Be(otherUserId);
        member.Role.Should().Be(BoardMemberRole.Member);
        member.JoinedAt.Should().Be(addTime);
        member.IsActive.Should().BeTrue();
        board.UpdatedAt.Should().Be(addTime);
    }

    [Fact]
    public void AddMember_WhenUserAlreadyActive_ShouldThrow()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        board.AddMember(userId, BoardMemberRole.Member, ownerId, now.AddMinutes(1));

        // Act
        var act = () => board.AddMember(userId, BoardMemberRole.Admin, ownerId, now.AddMinutes(2));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже является участником*");
    }

    [Fact]
    public void RemoveMember_ShouldDeactivateMember()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        var joinedAt = now.AddMinutes(1);
        board.AddMember(userId, BoardMemberRole.Member, ownerId, joinedAt);

        var removeTime = now.AddMinutes(10);

        // Act
        board.RemoveMember(userId, ownerId, removeTime);

        // Assert
        var member = board.Members.Single(m => m.UserId == userId);
        member.IsActive.Should().BeFalse();
        member.LeftAt.Should().Be(removeTime);
        board.UpdatedAt.Should().Be(removeTime);
    }

    [Fact]
    public void RemoveMember_WhenOwner_ShouldThrow()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        var removeTime = now.AddMinutes(5);

        // Act
        var act = () => board.RemoveMember(ownerId, ownerId, removeTime);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Нельзя удалить владельца доски*");

        var ownerMember = board.Members.Single();
        ownerMember.IsActive.Should().BeTrue();
        ownerMember.LeftAt.Should().BeNull();
    }

    [Fact]
    public void AddColumn_ShouldAssignIncrementalOrder()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Board", ownerId, now);

        // Act
        var firstColumn = board.AddColumn("Todo", ownerId, now.AddMinutes(1));
        var secondColumn = board.AddColumn("In progress", ownerId, now.AddMinutes(2));

        // Assert
        firstColumn.BoardId.Should().Be(board.Id);
        firstColumn.Order.Should().Be(0);

        secondColumn.BoardId.Should().Be(board.Id);
        secondColumn.Order.Should().Be(1);

        board.Columns.Should().HaveCount(2);
    }

    [Fact]
    public void CreateCard_ShouldAssignBoardAndColumnAndOrderWithinColumn()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Board", ownerId, now);

        var column1 = board.AddColumn("Todo", ownerId, now.AddMinutes(1));
        var column2 = board.AddColumn("Done", ownerId, now.AddMinutes(2));

        var userId = Guid.NewGuid();

        // Act
        var card1 = board.CreateCard(column1.Id, "Card 1", userId, now.AddMinutes(3));
        var card2 = board.CreateCard(column1.Id, "Card 2", userId, now.AddMinutes(4));
        var card3 = board.CreateCard(column2.Id, "Card 3", userId, now.AddMinutes(5));

        // Assert
        card1.BoardId.Should().Be(board.Id);
        card1.ColumnId.Should().Be(column1.Id);
        card1.Order.Should().Be(1);

        card2.BoardId.Should().Be(board.Id);
        card2.ColumnId.Should().Be(column1.Id);
        card2.Order.Should().Be(2);

        card3.BoardId.Should().Be(board.Id);
        card3.ColumnId.Should().Be(column2.Id);
        card3.Order.Should().Be(1);

        board.Cards.Should().HaveCount(3);
    }
    
    [Fact]
    public void ArchiveAndRestore_ShouldToggleFlagAndUpdateTimestamp()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var board = Board.Create("Board", ownerId, now);

        var archiveTime = now.AddMinutes(5);
        var restoreTime = now.AddMinutes(10);

        // Act
        board.Archive(archiveTime);

        // Assert
        board.IsArchived.Should().BeTrue();
        board.UpdatedAt.Should().Be(archiveTime);

        // Act
        board.Restore(restoreTime);

        // Assert
        board.IsArchived.Should().BeFalse();
        board.UpdatedAt.Should().Be(restoreTime);
    }

    [Fact]
    public void MoveCard_ShouldChangeColumnAndOrderAndUpdateTimestamp()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        var columnFrom = board.AddColumn("From", ownerId, now.AddMinutes(1));
        var columnTo = board.AddColumn("To", ownerId, now.AddMinutes(2));

        var card = board.CreateCard(columnFrom.Id, "Card", userId, now.AddMinutes(3));

        var moveTime = now.AddMinutes(4);

        // Act
        board.MoveCard(card.Id, columnTo.Id, targetOrder: 100, movedByUserId: userId, now: moveTime);

        // Assert
        card.ColumnId.Should().Be(columnTo.Id);
        card.Order.Should().Be(100);
        board.UpdatedAt.Should().Be(moveTime);
    }

    [Fact]
    public void ReorderCard_ShouldChangeOrderAndUpdateTimestamp()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        var column = board.AddColumn("Col", ownerId, now.AddMinutes(1));
        var card = board.CreateCard(column.Id, "Card", userId, now.AddMinutes(2));

        var reorderTime = now.AddMinutes(3);

        // Act
        board.ReorderCard(card.Id, newOrder: 50, reorderTime);

        // Assert
        card.Order.Should().Be(50);
        board.UpdatedAt.Should().Be(reorderTime);
    }

    [Fact]
    public void RemoveCard_ShouldRemoveCardAndUpdateTimestamp()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId, now);
        var column = board.AddColumn("Col", ownerId, now.AddMinutes(1));
        var card = board.CreateCard(column.Id, "Card", userId, now.AddMinutes(2));

        var removeTime = now.AddMinutes(3);

        // Act
        board.RemoveCard(card.Id, userId, removeTime);

        // Assert
        board.Cards.Should().NotContain(c => c.Id == card.Id);
        board.UpdatedAt.Should().Be(removeTime);
    }
}

