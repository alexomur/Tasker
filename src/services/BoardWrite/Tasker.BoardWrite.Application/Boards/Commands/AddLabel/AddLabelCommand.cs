using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.AddLabel;

/// <summary>
/// Команда на добавление метки на доску.
/// </summary>
/// <param name="BoardId">Идентификатор доски.</param>
/// <param name="Title">Название метки.</param>
/// <param name="Color">Цвет метки.</param>
/// <param name="Description">Описание метки, может быть null.</param>
public sealed record AddLabelCommand(
    Guid BoardId,
    string Title,
    string Color,
    string? Description
) : IRequest<AddLabelResult>;

/// <summary>
/// Результат добавления метки.
/// </summary>
/// <param name="LabelId">Идентификатор метки.</param>
/// <param name="Title">Название метки.</param>
/// <param name="Color">Цвет метки.</param>
/// <param name="Description">Описание метки.</param>
public sealed record AddLabelResult(
    Guid LabelId,
    string Title,
    string Color,
    string? Description);