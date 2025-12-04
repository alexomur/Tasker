using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;

/// <summary>
/// Команда на создание новой доски.
/// </summary>
/// <param name="Title">Название доски.</param>
/// <param name="Description">Описание доски, может быть null.</param>
public sealed record CreateBoardCommand(
    string Title,
    string? Description
) : IRequest<CreateBoardResult>;

/// <summary>
/// Результат создания доски.
/// </summary>
/// <param name="BoardId">Идентификатор созданной доски.</param>
public sealed record CreateBoardResult(Guid BoardId);