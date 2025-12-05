using FluentAssertions;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.UnitTests.BoardWrite;

public class CreateBoardHandlerTests
{
    private sealed class FakeCurrentUser : ICurrentUser
    {
        public FakeCurrentUser(bool isAuthenticated, Guid? userId)
        {
            IsAuthenticated = isAuthenticated;
            UserId = userId;
        }

        public bool IsAuthenticated { get; }

        public Guid? UserId { get; }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeBoardReadModelWriter : IBoardReadModelWriter
    {
        public List<Guid> RefreshedBoardIds { get; } = new();

        public Task RefreshBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
        {
            RefreshedBoardIds.Add(boardId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBoardRepository : IBoardRepository
    {
        private readonly List<Board> _boards = new();

        public IReadOnlyCollection<Board> Boards => _boards.AsReadOnly();

        public Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var board = _boards.SingleOrDefault(b => b.Id == id);
            return Task.FromResult(board);
        }

        public Task<Board?> GetByIdAsTrackingAsync(Guid id, CancellationToken ct = default)
        {
            // Для целей теста трекинг нам не важен — возвращаем то же самое, что и GetByIdAsync
            var board = _boards.SingleOrDefault(b => b.Id == id);
            return Task.FromResult(board);
        }

        public Task AddAsync(Board board, CancellationToken ct = default)
        {
            _boards.Add(board);
            return Task.CompletedTask;
        }

        public Task AddEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : Entity
        {
            // В этих тестах мы не используем AddEntityAsync, но сделаем простую поддержку досок
            if (entity is Board board)
            {
                _boards.Add(board);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Board>> GetBoardsForUserAsync(Guid userId, CancellationToken ct)
        {
            var result = _boards
                .Where(b => b.Members.Any(m => m.UserId == userId && m.IsActive))
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Board>>(result);
        }
    }

    [Fact]
    public async Task Handle_ShouldCreateBoardAndCallDependencies()
    {
        // Arrange
        var repo = new FakeBoardRepository();
        var uow = new FakeUnitOfWork();
        var currentUserId = Guid.NewGuid();
        var currentUser = new FakeCurrentUser(isAuthenticated: true, userId: currentUserId);
        var readModelWriter = new FakeBoardReadModelWriter();

        var handler = new CreateBoardHandler(repo, uow, currentUser, readModelWriter);

        var cmd = new CreateBoardCommand(
            Title: "  My Board  ",
            Description: "  Description  ");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.BoardId.Should().NotBe(Guid.Empty);

        repo.Boards.Should().HaveCount(1);
        var board = repo.Boards.Single();

        board.Id.Should().Be(result.BoardId);
        board.Title.Should().Be("My Board");
        board.Description.Should().Be("Description");
        board.OwnerUserId.Should().Be(currentUserId);

        board.Members
            .Should()
            .ContainSingle(m => m.UserId == currentUserId &&
                                m.Role == BoardMemberRole.Owner &&
                                m.IsActive);

        uow.SaveChangesCallCount.Should().Be(1);
        readModelWriter.RefreshedBoardIds.Should().ContainSingle(id => id == board.Id);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowAndNotTouchDependencies()
    {
        // Arrange
        var repo = new FakeBoardRepository();
        var uow = new FakeUnitOfWork();
        var currentUser = new FakeCurrentUser(isAuthenticated: false, userId: null);
        var readModelWriter = new FakeBoardReadModelWriter();

        var handler = new CreateBoardHandler(repo, uow, currentUser, readModelWriter);

        var cmd = new CreateBoardCommand(
            Title: "Board",
            Description: null);

        // Act
        Func<Task> act = () => handler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Текущий пользователь не определён.");

        repo.Boards.Should().BeEmpty();
        uow.SaveChangesCallCount.Should().Be(0);
        readModelWriter.RefreshedBoardIds.Should().BeEmpty();
    }
}
