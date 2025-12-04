using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.MoveCard;

/// <summary>
/// Команда на перемещение карточки в другую колонку.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="TargetColumnId">Целевая колонка, в которую нужно переместить карточку.</param>
public sealed record MoveCardCommand(
    Guid BoardId,
    Guid CardId,
    Guid TargetColumnId
) : IRequest<MoveCardResult>;

/// <summary>
/// Результат перемещения карточки.
/// </summary>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="ColumnId">Идентификатор колонки, куда переместили.</param>
/// <param name="Order">Новый порядок карточки в колонке.</param>
public sealed record MoveCardResult(
    Guid CardId,
    Guid ColumnId,
    int Order);