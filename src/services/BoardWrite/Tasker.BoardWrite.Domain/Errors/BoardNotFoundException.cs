using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors;

/// <summary>
/// Ошибка, возникающая, когда запрашиваемая доска не найдена.
/// </summary>
public sealed class BoardNotFoundException : AppException
{
    public BoardNotFoundException(Guid boardId)
        : base(
            message: $"Board '{boardId}' not found.",
            code: "board.not_found",
            statusCode: 404)
    {
        BoardId = boardId;
    }

    public Guid BoardId { get; }
}