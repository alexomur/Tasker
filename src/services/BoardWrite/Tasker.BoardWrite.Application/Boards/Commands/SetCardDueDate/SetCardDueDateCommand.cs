using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.SetCardDueDate;

/// <summary>
/// Команда на установку или сброс дедлайна карточки.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="DueDate">
/// Новая дата дедлайна (UTC). Null — сбросить дедлайн.
/// </param>
public sealed record SetCardDueDateCommand(
    Guid BoardId,
    Guid CardId,
    DateTimeOffset? DueDate
) : IRequest<SetCardDueDateResult>;

/// <summary>
/// Результат установки дедлайна.
/// </summary>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="DueDate">Фактически установленный дедлайн.</param>
public sealed record SetCardDueDateResult(
    Guid CardId,
    DateTimeOffset? DueDate);