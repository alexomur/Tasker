using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.UpdateCard;

/// <summary>
/// Команда на обновление карточки на доске.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="CardId">Идентификатор карточки.</param>
/// <param name="Title">Новый заголовок карточки.</param>
/// <param name="Description">Новое описание карточки, может быть null.</param>
public sealed record UpdateCardCommand(
    Guid BoardId,
    Guid CardId,
    string Title,
    string? Description
) : IRequest<UpdateCardResult>;

/// <summary>
/// Результат обновления карточки.
/// </summary>
/// <param name="CardId">Идентификатор обновлённой карточки.</param>
public sealed record UpdateCardResult(Guid CardId);