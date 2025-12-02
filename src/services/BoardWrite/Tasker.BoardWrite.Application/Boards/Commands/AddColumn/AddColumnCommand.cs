using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddColumn;

/// <summary>
/// Команда на добавление новой колонки на доску.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="Title">Название колонки.</param>
/// <param name="Description">Описание колонки, может быть null.</param>
public sealed record AddColumnCommand(
    Guid BoardId,
    string Title,
    string? Description
) : IRequest<AddColumnResult>;

/// <summary>
/// Результат добавления колонки.
/// </summary>
/// <param name="ColumnId">Идентификатор созданной колонки.</param>
/// <param name="Title">Название колонки.</param>
/// <param name="Description">Описание колонки.</param>
/// <param name="Order">Порядок колонки.</param>
public sealed record AddColumnResult(
    Guid ColumnId,
    string Title,
    string? Description,
    int Order);