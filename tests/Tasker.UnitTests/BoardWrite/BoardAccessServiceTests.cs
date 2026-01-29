using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.BoardWrite.Infrastructure;
using Tasker.BoardWrite.Infrastructure.Security;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.UnitTests.BoardWrite;

internal sealed class TestCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; init; }
    public Guid? UserId { get; init; }
}

public class BoardAccessServiceTests
{
    private static BoardWriteDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BoardWriteDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BoardWriteDbContext(options);
    }

    [Fact]
    public async Task EnsureCanReadWriteManage_ShouldSucceed_ForOwner()
    {
        // Arrange
        var dbName = $"BoardAccess_Owner_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerUserId = Guid.NewGuid();

        var board = Board.Create("Board", ownerUserId, now);
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUser
        {
            IsAuthenticated = true,
            UserId = ownerUserId
        };

        var service = new BoardAccessService(db, currentUser);

        // Act
        var readAct = async () => await service.EnsureCanReadBoardAsync(board.Id, CancellationToken.None);
        var writeAct = async () => await service.EnsureCanWriteBoardAsync(board.Id, CancellationToken.None);
        var manageAct = async () => await service.EnsureCanManageMembersAsync(board.Id, CancellationToken.None);

        // Assert
        await readAct.Should().NotThrowAsync();
        await writeAct.Should().NotThrowAsync();
        await manageAct.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Viewer_ShouldBeAbleToRead_ButNotWrite()
    {
        // Arrange
        var dbName = $"BoardAccess_Viewer_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerUserId = Guid.NewGuid();
        var viewerUserId = Guid.NewGuid();

        var board = Board.Create("Board", ownerUserId, now);
        board.AddMember(viewerUserId, BoardMemberRole.Viewer, ownerUserId, now);

        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUser
        {
            IsAuthenticated = true,
            UserId = viewerUserId
        };

        var service = new BoardAccessService(db, currentUser);

        // Act
        var readAct = async () => await service.EnsureCanReadBoardAsync(board.Id, CancellationToken.None);
        var writeAct = async () => await service.EnsureCanWriteBoardAsync(board.Id, CancellationToken.None);

        // Assert
        await readAct.Should().NotThrowAsync();
        await writeAct.Should().ThrowAsync<BoardAccessDeniedException>();
    }

    [Fact]
    public async Task NonMember_ShouldGetAccessDenied_OnRead()
    {
        // Arrange
        var dbName = $"BoardAccess_NonMember_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var board = Board.Create("Board", ownerUserId, now);
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUser
        {
            IsAuthenticated = true,
            UserId = otherUserId
        };

        var service = new BoardAccessService(db, currentUser);

        // Act
        var act = async () => await service.EnsureCanReadBoardAsync(board.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BoardAccessDeniedException>();
    }

    [Fact]
    public async Task UnauthenticatedUser_ShouldGetAccessDenied()
    {
        // Arrange
        var dbName = $"BoardAccess_Unauth_{Guid.NewGuid()}";
        await using var db = CreateDbContext(dbName);

        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ownerUserId = Guid.NewGuid();

        var board = Board.Create("Board", ownerUserId, now);
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var currentUser = new TestCurrentUser
        {
            IsAuthenticated = false,
            UserId = null
        };

        var service = new BoardAccessService(db, currentUser);

        // Act
        var act = async () => await service.EnsureCanReadBoardAsync(board.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BoardAccessDeniedException>()
            .Where(ex => ex.UserId == Guid.Empty);
    }
}

