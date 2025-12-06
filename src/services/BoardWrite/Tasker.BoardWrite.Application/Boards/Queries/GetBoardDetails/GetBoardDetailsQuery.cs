using MediatR;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.BoardWrite.Application.Boards.Queries.GetBoardDetails;

/// <summary>
/// Запрос на получение полной информации о доске.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
public sealed record GetBoardDetailsQuery(Guid BoardId)
    : IRequest<BoardDetailsResult>;

/// <summary>
/// Детальная информация о доске, включающая колонки, участников, метки и карточки.
/// </summary>
public sealed record BoardDetailsResult(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerUserId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<BoardColumnDto> Columns,
    IReadOnlyCollection<BoardMemberDto> Members,
    IReadOnlyCollection<BoardLabelDto> Labels,
    IReadOnlyCollection<BoardCardDto> Cards);

/// <summary>
/// DTO для колонки на доске.
/// </summary>
public sealed record BoardColumnDto(
    Guid Id,
    string Title,
    string? Description,
    int Order);

/// <summary>
/// DTO для участника доски.
/// </summary>
public sealed record BoardMemberDto(
    Guid Id,
    Guid UserId,
    BoardMemberRole Role,
    bool IsActive,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LeftAt);

/// <summary>
/// DTO для метки на доске.
/// </summary>
public sealed record BoardLabelDto(
    Guid Id,
    string Title,
    string? Description,
    string Color);

/// <summary>
/// DTO для карточки на доске.
/// </summary>
public sealed record BoardCardDto(
    Guid Id,
    Guid ColumnId,
    string Title,
    string? Description,
    int Order,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DueDate,
    IReadOnlyCollection<Guid> AssigneeUserIds,
    IReadOnlyCollection<Guid> LabelIds);