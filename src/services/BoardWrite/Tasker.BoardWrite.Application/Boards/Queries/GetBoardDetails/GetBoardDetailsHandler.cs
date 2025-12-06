using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Domain.Errors;

namespace Tasker.BoardWrite.Application.Boards.Queries.GetBoardDetails;

/// <summary>
/// Обработчик запроса получения полной информации о доске.
/// </summary>
public sealed class GetBoardDetailsHandler
    : IRequestHandler<GetBoardDetailsQuery, BoardDetailsResult>
{
    private readonly IBoardRepository _boards;
    private readonly IBoardAccessService _boardAccess;

    public GetBoardDetailsHandler(IBoardRepository boards, IBoardAccessService boardAccess)
    {
        _boards = boards;
        _boardAccess = boardAccess;
    }

    public async Task<BoardDetailsResult> Handle(GetBoardDetailsQuery request, CancellationToken ct)
    {
        var board = await _boards.GetByIdAsTrackingAsync(request.BoardId, ct);
        if (board is null)
        {
            throw new BoardNotFoundException(request.BoardId);
        }

        await _boardAccess.EnsureCanReadBoardAsync(board.Id, ct);

        var columns = board.Columns
            .OrderBy(c => c.Order)
            .Select(c => new BoardColumnDto(
                Id: c.Id,
                Title: c.Title,
                Description: c.Description,
                Order: c.Order))
            .ToList();

        var members = board.Members
            .Select(m => new BoardMemberDto(
                Id: m.Id,
                UserId: m.UserId,
                Role: m.Role,
                IsActive: m.IsActive,
                JoinedAt: m.JoinedAt,
                LeftAt: m.LeftAt))
            .ToList();

        var labels = board.Labels
            .Select(l => new BoardLabelDto(
                Id: l.Id,
                Title: l.Title,
                Description: l.Description,
                Color: l.Color))
            .ToList();

        var cards = board.Cards
            .Select(c => new BoardCardDto(
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

        return new BoardDetailsResult(
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
            Cards: cards);
    }
}
