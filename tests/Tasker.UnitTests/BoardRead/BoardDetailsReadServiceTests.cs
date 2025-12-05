using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasker.BoardRead.Application.Boards.Views;
using Tasker.BoardRead.Infrastructure.Boards;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.BoardWrite.Infrastructure;
using Tasker.Shared.Kernel.Abstractions.ReadModel;
using WriteBoardMemberRole = Tasker.BoardWrite.Domain.Boards.BoardMemberRole;
using ReadBoardMemberRole = Tasker.BoardRead.Application.Boards.Views.BoardMemberRole;

namespace Tasker.UnitTests.BoardRead;

public class BoardDetailsReadServiceTests
{
    private sealed class FakeBoardSnapshotStore : IBoardSnapshotStore
    {
        private readonly Dictionary<Guid, string> _store = new();

        public List<Guid> TryGetCalls { get; } = new();
        public List<(Guid BoardId, string Payload, int TtlSeconds)> Upserts { get; } = new();

        public void Seed(Guid boardId, string json) => _store[boardId] = json;

        public Task<string?> TryGetAsync(Guid boardId, CancellationToken cancellationToken = default)
        {
            TryGetCalls.Add(boardId);
            _store.TryGetValue(boardId, out var json);
            return Task.FromResult(json);
        }

        public Task UpsertAsync(Guid boardId, string payloadJson, int ttlSeconds, CancellationToken cancellationToken = default)
        {
            Upserts.Add((boardId, payloadJson, ttlSeconds));
            _store[boardId] = payloadJson;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBoardAccessService : IBoardAccessService
    {
        public List<Guid> EnsureCanReadCalls { get; } = new();

        public Task EnsureCanReadBoardAsync(Guid boardId, CancellationToken ct)
        {
            EnsureCanReadCalls.Add(boardId);
            return Task.CompletedTask;
        }

        public Task EnsureCanWriteBoardAsync(Guid boardId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task EnsureCanManageMembersAsync(Guid boardId, CancellationToken ct)
            => throw new NotImplementedException();
    }

    private sealed class NullLogger<T> : ILogger<T>
    {
        public static readonly NullLogger<T> Instance = new();

        private NullLogger() { }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // no-op
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task GetBoardAsync_WhenSnapshotExists_ShouldReturnSnapshotAndNotWriteNew()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        var snapshotStore = new FakeBoardSnapshotStore();
        var accessService = new FakeBoardAccessService();
        var logger = NullLogger<BoardDetailsReadService>.Instance;

        var columns = new[]
        {
            new BoardColumnView(Guid.NewGuid(), "Todo", null, 0)
        };

        var members = new[]
        {
            new BoardMemberView(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                Role: ReadBoardMemberRole.Owner,
                IsActive: true,
                JoinedAt: new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                LeftAt: null)
        };

        var labels = new[]
        {
            new BoardLabelView(Guid.NewGuid(), "Bug", "Bug description", "#ff0000")
        };

        var cards = new[]
        {
            new BoardCardView(
                Id: Guid.NewGuid(),
                ColumnId: columns[0].Id,
                Title: "Card 1",
                Description: "Desc",
                Order: 1,
                CreatedByUserId: members[0].UserId,
                CreatedAt: new DateTimeOffset(2025, 1, 1, 12, 5, 0, TimeSpan.Zero),
                UpdatedAt: new DateTimeOffset(2025, 1, 1, 12, 10, 0, TimeSpan.Zero),
                DueDate: null,
                AssigneeUserIds: Array.Empty<Guid>())
        };

        var snapshotView = new BoardDetailsView(
            Id: boardId,
            Title: "Board From Snapshot",
            Description: "Snapshot description",
            OwnerUserId: members[0].UserId,
            IsArchived: false,
            CreatedAt: new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            UpdatedAt: new DateTimeOffset(2025, 1, 1, 13, 0, 0, TimeSpan.Zero),
            Columns: columns,
            Members: members,
            Labels: labels,
            Cards: cards);

        var json = JsonSerializer.Serialize(snapshotView, JsonOptions);
        snapshotStore.Seed(boardId, json);

        var dbOptions = new DbContextOptionsBuilder<BoardWriteDbContext>()
            .UseInMemoryDatabase("BoardDetails_SnapshotExists")
            .Options;

        using var dbContext = new BoardWriteDbContext(dbOptions);

        var service = new BoardDetailsReadService(
            snapshotStore,
            dbContext,
            accessService,
            logger);

        // Act
        var result = await service.GetBoardAsync(boardId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(snapshotView);

        snapshotStore.Upserts.Should().BeEmpty();
        accessService.EnsureCanReadCalls.Should().ContainSingle(id => id == boardId);
    }

    [Fact]
    public async Task GetBoardAsync_WhenSnapshotMissing_ShouldLoadFromDbAndCreateSnapshot()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<BoardWriteDbContext>()
            .UseInMemoryDatabase("BoardDetails_SnapshotMissing")
            .Options;

        Guid boardId;
        Guid ownerId;
        Guid assigneeId;
        DateTimeOffset now = new(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Сидируем доску в InMemory-DB
        using (var seedContext = new BoardWriteDbContext(dbOptions))
        {
            ownerId = Guid.NewGuid();
            assigneeId = Guid.NewGuid();

            var board = Board.Create(
                title: "My Board",
                ownerUserId: ownerId,
                now: now,
                description: "Board description");

            var column = board.AddColumn("Todo", now, "Todo column");

            var card = board.CreateCard(
                columnId: column.Id,
                title: "First card",
                createdByUserId: ownerId,
                now: now.AddMinutes(5),
                description: "Card description",
                dueDate: now.AddDays(1));

            card.AssignUser(assigneeId, now.AddMinutes(10));

            // Добавим assignee в участники доски
            board.AddMember(assigneeId, WriteBoardMemberRole.Member, now.AddMinutes(2));

            // Добавим метку
            board.AddLabel("Bug", "#ff0000", "Bug label");

            seedContext.Boards.Add(board);
            await seedContext.SaveChangesAsync();
            boardId = board.Id;
        }

        var snapshotStore = new FakeBoardSnapshotStore();
        var accessService = new FakeBoardAccessService();
        var logger = NullLogger<BoardDetailsReadService>.Instance;

        using var dbContext = new BoardWriteDbContext(dbOptions);

        var service = new BoardDetailsReadService(
            snapshotStore,
            dbContext,
            accessService,
            logger);

        // Act
        var result = await service.GetBoardAsync(boardId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(boardId);
        result.Title.Should().Be("My Board");
        result.Description.Should().Be("Board description");
        result.OwnerUserId.Should().Be(ownerId);

        result.Columns.Should().HaveCount(1);
        result.Members.Should().HaveCount(2); // владелец + assignee
        result.Labels.Should().HaveCount(1);
        result.Cards.Should().HaveCount(1);

        var cardView = result.Cards.Single();
        cardView.Title.Should().Be("First card");
        cardView.AssigneeUserIds.Should().ContainSingle(id => id == assigneeId);

        // Снапшот должен быть записан
        snapshotStore.Upserts.Should().ContainSingle(u => u.BoardId == boardId);
        var upsert = snapshotStore.Upserts.Single();

        upsert.TtlSeconds.Should().Be(24 * 60 * 60);
        upsert.Payload.Should().NotBeNullOrWhiteSpace();

        // Должен был вызваться EnsureCanReadBoardAsync
        accessService.EnsureCanReadCalls.Should().ContainSingle(id => id == boardId);

        // И TryGetAsync тоже
        snapshotStore.TryGetCalls.Should().ContainSingle(id => id == boardId);
    }
}
