using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tasker.BoardRead.Infrastructure.Boards;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Infrastructure;
using Tasker.Shared.Kernel.Abstractions;
using DomainBoardMemberRole = Tasker.BoardWrite.Domain.Boards.BoardMemberRole;
using ReadBoardMemberRole = Tasker.BoardRead.Application.Boards.Views.BoardMemberRole;

namespace Tasker.UnitTests.BoardRead;

internal sealed class TestCurrentUserForRead : ICurrentUser
{
    public bool IsAuthenticated { get; init; }
    public Guid? UserId { get; init; }
}

public class BoardListReadServiceTests
{
    private static BoardWriteDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BoardWriteDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BoardWriteDbContext(options);
    }

    [Fact]
    public async Task GetMyBoardsAsync_ShouldReturnBoardsWhereUserIsActiveMember_WithCorrectRoles()
    {
        // Arrange
        var dbName = $"BoardList_MyBoards_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Доска, где пользователь — владелец
        var ownedBoard = Board.Create("Owned board", currentUserId, now);

        // Доска, где пользователь — Member
        var memberBoard = Board.Create("Member board", otherUserId, now);
        memberBoard.AddMember(currentUserId, DomainBoardMemberRole.Member, now);

        // Доска, где пользователя нет
        var otherBoard = Board.Create("Other board", otherUserId, now);

        db.Boards.AddRange(ownedBoard, memberBoard, otherBoard);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUserForRead
        {
            IsAuthenticated = true,
            UserId = currentUserId
        };

        var service = new BoardListReadService(
            db,
            currentUser,
            NullLogger<BoardListReadService>.Instance);

        // Act
        var result = await service.GetMyBoardsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        var ownedView = result.Single(b => b.Id == ownedBoard.Id);
        ownedView.MyRole.Should().Be(ReadBoardMemberRole.Owner);
        ownedView.OwnerUserId.Should().Be(currentUserId);

        var memberView = result.Single(b => b.Id == memberBoard.Id);
        memberView.MyRole.Should().Be(ReadBoardMemberRole.Member);
        memberView.OwnerUserId.Should().Be(otherUserId);

        // Убедимся, что лишняя доска не попала
        result.Select(b => b.Id).Should().NotContain(otherBoard.Id);
    }

    [Fact]
    public async Task GetMyBoardsAsync_ShouldThrow_WhenCurrentUserIsNull()
    {
        // Arrange
        var dbName = $"BoardList_NoUser_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var currentUser = new TestCurrentUserForRead
        {
            IsAuthenticated = false,
            UserId = null
        };

        var service = new BoardListReadService(
            db,
            currentUser,
            NullLogger<BoardListReadService>.Instance);

        // Act
        var act = async () => await service.GetMyBoardsAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetMyBoardsAsync_ShouldReturnViewerRole_WhenMembershipIsInactive()
    {
        // Arrange
        var dbName = $"BoardList_Inactive_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var userId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        var board = Board.Create("Old board", ownerUserId, now);
        var member = board.AddMember(userId, DomainBoardMemberRole.Member, now);
        member.Leave(now.AddHours(1)); // делаем участие неактивным

        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUserForRead
        {
            IsAuthenticated = true,
            UserId = userId
        };

        var service = new BoardListReadService(
            db,
            currentUser,
            NullLogger<BoardListReadService>.Instance);

        // Act
        var result = await service.GetMyBoardsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        var view = result.Single();
        view.Id.Should().Be(board.Id);

        // Так как активного membership нет, сервис вернёт роль Viewer
        view.MyRole.Should().Be(ReadBoardMemberRole.Viewer);
    }
}
