using FluentAssertions;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Application.Boards.Queries.GetBoardDetails;
using Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;
using Tasker.BoardRead.Application.Boards.Views;
using Tasker.BoardRead.Application.Users.Views;

namespace Tasker.UnitTests.BoardRead;

public class BoardReadQueriesTests
{
    private sealed class FakeBoardDetailsReadService : IBoardDetailsReadService
    {
        private readonly BoardDetailsView? _result;

        public FakeBoardDetailsReadService(BoardDetailsView? result)
        {
            _result = result;
        }

        public Guid? LastRequestedBoardId { get; private set; }

        public Task<BoardDetailsView?> GetBoardAsync(
            Guid boardId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedBoardId = boardId;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeBoardListReadService : IBoardListReadService
    {
        private readonly IReadOnlyCollection<BoardView> _result;

        public FakeBoardListReadService(IReadOnlyCollection<BoardView> result)
        {
            _result = result;
        }

        public bool WasCalled { get; private set; }

        public Task<IReadOnlyCollection<BoardView>> GetMyBoardsAsync(
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(_result);
        }
    }

    [Fact]
    public async Task GetBoardDetailsHandler_ShouldCallServiceAndReturnResult()
    {
        var boardId = Guid.NewGuid();

        var view = new BoardDetailsView(
            Id: boardId,
            Title: "Test board",
            Description: "Desc",
            OwnerUserId: Guid.NewGuid(),
            IsArchived: false,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow,
            Columns: Array.Empty<BoardColumnView>(),
            Members: Array.Empty<BoardMemberView>(),
            Labels: Array.Empty<BoardLabelView>(),
            Cards: Array.Empty<BoardCardView>(),
            Users: Array.Empty<UserView>());

        var service = new FakeBoardDetailsReadService(view);
        var handler = new GetBoardDetailsHandler(service);

        var result = await handler.Handle(
            new GetBoardDetailsQuery(boardId),
            CancellationToken.None);

        result.Should().BeSameAs(view);
        service.LastRequestedBoardId.Should().Be(boardId);
    }

    [Fact]
    public async Task GetMyBoardsHandler_ShouldCallServiceAndReturnResult()
    {
        var boards = new[]
        {
            new BoardView(
                Id: Guid.NewGuid(),
                Title: "Board 1",
                Description: null,
                OwnerUserId: Guid.NewGuid(),
                IsArchived: false,
                CreatedAt: DateTimeOffset.UtcNow,
                UpdatedAt: DateTimeOffset.UtcNow,
                MyRole: BoardMemberRole.Owner)
        };

        var service = new FakeBoardListReadService(boards);
        var handler = new GetMyBoardsHandler(service);

        var result = await handler.Handle(
            new GetMyBoardsQuery(),
            CancellationToken.None);

        result.Should().BeEquivalentTo(boards);
        service.WasCalled.Should().BeTrue();
    }
}
