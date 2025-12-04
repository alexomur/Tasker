using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateCard;

/// <summary>
/// Команда на создание новой карточки в колонке.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="ColumnId">Идентификатор колонки.</param>
/// <param name="Title">Заголовок карточки.</param>
/// <param name="Description">Описание карточки, может быть null.</param>
/// <param name="DueDate">Дедлайн карточки, может быть null.</param>
public sealed record CreateCardCommand(
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    DateTimeOffset? DueDate
) : IRequest<CreateCardResult>;

/// <summary>
/// Результат создания карточки.
/// </summary>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="ColumnId">Идентификатор колонки.</param>
/// <param name="Order">Порядок карточки в колонке.</param>
public sealed record CreateCardResult(
    Guid CardId,
    Guid ColumnId,
    int Order);