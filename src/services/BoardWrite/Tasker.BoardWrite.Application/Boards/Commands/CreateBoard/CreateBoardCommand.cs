using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;

/// <summary>
/// Команда на создание новой доски.
/// </summary>
/// <param name="Title">Название доски.</param>
/// <param name="Description">Описание доски, может быть null.</param>
/// <param name="TemplateCode">
/// Код шаблона доски (например, "default/software", "gamedev/feature").
/// Если null или пустая строка — доска создаётся пустой.
/// </param>
public sealed record CreateBoardCommand(
    string Title,
    string? Description,
    string? TemplateCode
) : IRequest<CreateBoardResult>;

/// <summary>
/// Результат создания доски.
/// </summary>
/// <param name="BoardId">Идентификатор созданной доски.</param>
public sealed record CreateBoardResult(Guid BoardId);