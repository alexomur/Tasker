using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Domain.Errors;
using Tasker.BoardWrite.Infrastructure;
using Tasker.BoardWrite.Infrastructure.Security;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.UnitTests.BoardWrite;

public class BoardAccessServiceTests
{
    private static BoardWriteDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BoardWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BoardWriteDbContext(options);
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public TestCurrentUser(bool isAuthenticated, Guid? userId)
        {
            IsAuthenticated = isAuthenticated;
            UserId = userId;
        }

        public bool IsAuthenticated { get; }

        public Guid? UserId { get; }
    }

    private static async Task SeedBoardMemberAsync(
        BoardWriteDbContext dbContext,
        Guid boardId,
        Guid userId,
        BoardMemberRole role,
        bool left = false)
    {
        var joinedAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var member = new BoardMember(boardId, userId, role, joinedAt);

        if (left)
        {
            var leftAt = joinedAt.AddMinutes(10);
            member.Leave(leftAt);
        }

        dbContext.BoardMembers.Add(member);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task EnsureCanReadBoardAsync_WhenUserNotAuthenticated_ShouldThrowAccessDenied()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();
        var currentUser = new TestCurrentUser(isAuthenticated: false, userId: null);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act
        Func<Task> act = () => service.EnsureCanReadBoardAsync(boardId, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BoardAccessDeniedException>();
        ex.Which.BoardId.Should().Be(boardId);
        ex.Which.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task EnsureCanReadBoardAsync_WhenUserIsNotMember_ShouldThrowAccessDenied()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        // никаких записей BoardMembers для данного пользователя и доски
        var currentUser = new TestCurrentUser(isAuthenticated: true, userId: userId);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act
        Func<Task> act = () => service.EnsureCanReadBoardAsync(boardId, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BoardAccessDeniedException>();
        ex.Which.BoardId.Should().Be(boardId);
        ex.Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Owner_ShouldHaveFullAccess()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedBoardMemberAsync(dbContext, boardId, userId, BoardMemberRole.Owner);

        var currentUser = new TestCurrentUser(isAuthenticated: true, userId: userId);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act / Assert
        await service.Invoking(s => s.EnsureCanReadBoardAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();

        await service.Invoking(s => s.EnsureCanWriteBoardAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();

        await service.Invoking(s => s.EnsureCanManageMembersAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Member_ShouldReadAndWrite_ButNotManageMembers()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedBoardMemberAsync(dbContext, boardId, userId, BoardMemberRole.Member);

        var currentUser = new TestCurrentUser(isAuthenticated: true, userId: userId);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act / Assert
        await service.Invoking(s => s.EnsureCanReadBoardAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();

        await service.Invoking(s => s.EnsureCanWriteBoardAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();

        var actManage = () => service.EnsureCanManageMembersAsync(boardId, CancellationToken.None);
        var ex = await actManage.Should().ThrowAsync<BoardAccessDeniedException>();
        ex.Which.BoardId.Should().Be(boardId);
        ex.Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Viewer_ShouldOnlyRead()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedBoardMemberAsync(dbContext, boardId, userId, BoardMemberRole.Viewer);

        var currentUser = new TestCurrentUser(isAuthenticated: true, userId: userId);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act / Assert
        await service.Invoking(s => s.EnsureCanReadBoardAsync(boardId, CancellationToken.None))
            .Should().NotThrowAsync();

        var actWrite = () => service.EnsureCanWriteBoardAsync(boardId, CancellationToken.None);
        var exWrite = await actWrite.Should().ThrowAsync<BoardAccessDeniedException>();
        exWrite.Which.BoardId.Should().Be(boardId);
        exWrite.Which.UserId.Should().Be(userId);

        var actManage = () => service.EnsureCanManageMembersAsync(boardId, CancellationToken.None);
        var exManage = await actManage.Should().ThrowAsync<BoardAccessDeniedException>();
        exManage.Which.BoardId.Should().Be(boardId);
        exManage.Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task MemberWithLeftAt_ShouldBeTreatedAsNoAccess()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        await SeedBoardMemberAsync(dbContext, boardId, userId, BoardMemberRole.Member, left: true);

        var currentUser = new TestCurrentUser(isAuthenticated: true, userId: userId);
        var service = new BoardAccessService(dbContext, currentUser);

        // Act
        Func<Task> act = () => service.EnsureCanReadBoardAsync(boardId, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BoardAccessDeniedException>();
        ex.Which.BoardId.Should().Be(boardId);
        ex.Which.UserId.Should().Be(userId);
    }
}
