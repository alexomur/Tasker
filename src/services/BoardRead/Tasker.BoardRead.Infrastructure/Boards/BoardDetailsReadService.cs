using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Application.Users.Abstractions;
using Tasker.BoardRead.Domain.BoardViews;
using Tasker.BoardRead.Domain.UserViews;
// TODO: Write a separate module for BoardAccessService
using Tasker.BoardWrite.Infrastructure;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.Shared.Kernel.Abstractions.ReadModel;

namespace Tasker.BoardRead.Infrastructure.Boards;

using ReadBoardMemberRole = BoardMemberRole;

/// <summary>
/// Реализация чтения доски: сначала Cassandra, затем fallback в MySQL (BoardWriteDbContext).
/// </summary>
public sealed class BoardDetailsReadService : IBoardDetailsReadService
{
    private readonly IBoardSnapshotStore _snapshots;
    private readonly BoardWriteDbContext _dbContext;
    private readonly IBoardAccessService _boardAccessService;
    private readonly IUserReadService _userReadService;
    private readonly ILogger<BoardDetailsReadService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const int SnapshotTtlSeconds = 24 * 60 * 60; // 24 часа

    public BoardDetailsReadService(
        IBoardSnapshotStore snapshots,
        BoardWriteDbContext dbContext,
        IBoardAccessService boardAccessService,
        ILogger<BoardDetailsReadService> logger, IUserReadService userReadService)
    {
        _snapshots = snapshots;
        _dbContext = dbContext;
        _boardAccessService = boardAccessService;
        _logger = logger;
        _userReadService = userReadService;
    }

    public async Task<BoardDetailsView?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        await _boardAccessService.EnsureCanReadBoardAsync(boardId, cancellationToken);

        // Пытаемся прочитать снапшот из Cassandra
        BoardDetailsView? fromSnapshot = null;
        try
        {
            var json = await _snapshots.TryGetAsync(boardId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(json))
            {
                fromSnapshot = JsonSerializer.Deserialize<BoardDetailsView>(json, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read board snapshot from Cassandra for board {BoardId}, will fallback to MySQL",
                boardId);
        }

        if (fromSnapshot is not null)
        {
            return await EnrichWithUsersAsync(fromSnapshot, cancellationToken);
        }

        // Fallback: читаем из MySQL через BoardWriteDbContext
        _logger.LogInformation("Board snapshot for {BoardId} not found, loading from MySQL", boardId);
        
        var board = await _dbContext.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .Include(b => b.Cards)
            .ThenInclude(c => c.Labels)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);

        if (board is null)
        {
            _logger.LogWarning("Board {BoardId} not found in MySQL", boardId);
            return null;
        }

        var columns = board.Columns
            .OrderBy(c => c.Order)
            .Select(c => new BoardColumnView(
                Id: c.Id,
                Title: c.Title,
                Description: c.Description,
                Order: c.Order))
            .ToList();

        var members = board.Members
            .Select(m => new BoardMemberView(
                Id: m.Id,
                UserId: m.UserId,
                Role: (ReadBoardMemberRole)(int)m.Role,
                IsActive: m.IsActive,
                JoinedAt: m.JoinedAt,
                LeftAt: m.LeftAt))
            .ToList();

        var labels = board.Labels
            .Select(l => new BoardLabelView(
                Id: l.Id,
                Title: l.Title,
                Description: l.Description,
                Color: l.Color))
            .ToList();

        var cards = board.Cards
            .Select(c => new BoardCardView(
                Id: c.Id,
                ColumnId: c.ColumnId,
                Title: c.Title,
                Description: c.Description,
                Order: c.Order,
                CreatedByUserId: c.CreatedByUserId,
                CreatedAt: c.CreatedAt,
                UpdatedAt: c.UpdatedAt,
                DueDate: c.DueDate,
                AssigneeUserIds: c.AssigneeUserIds.ToArray(),
                LabelIds: c.Labels.Select(l => l.Id).ToArray()))
            .ToList();

        var baseView = new BoardDetailsView(
            Id: board.Id,
            Title: board.Title,
            Description: board.Description,
            OwnerUserId: board.OwnerUserId,
            IsArchived: board.IsArchived,
            CreatedAt: board.CreatedAt,
            UpdatedAt: board.UpdatedAt,
            Columns: columns,
            Members: members,
            Labels: labels,
            Cards: cards,
            Users: Array.Empty<UserView>());

        var view = await EnrichWithUsersAsync(baseView, cancellationToken);

        // Пишем свежий снапшот в Cassandra
        try
        {
            var json = JsonSerializer.Serialize(view, JsonOptions);
            await _snapshots.UpsertAsync(boardId, json, SnapshotTtlSeconds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to upsert board snapshot for board {BoardId} to Cassandra", boardId);
        }

        return view;
    }
    
    private async Task<BoardDetailsView> EnrichWithUsersAsync(
        BoardDetailsView view,
        CancellationToken cancellationToken)
    {
        var userIds = view.Members
            .Select(m => m.UserId)
            .Append(view.OwnerUserId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (userIds.Length == 0)
        {
            return view with { Users = Array.Empty<UserView>() };
        }

        var users = await _userReadService.GetByIdsAsync(userIds, cancellationToken);

        // Для стабильности можем отсортировать по имени
        var orderedUsers = users
            .OrderBy(u => u.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return view with { Users = orderedUsers };
    }
}
