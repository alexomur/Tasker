using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors;

/// <summary>
/// Исключение, выбрасываемое при попытке обращения к несуществующей колонке.
/// </summary>
public sealed class ColumnNotFoundException : AppException
{
    public Guid ColumnId { get; }

    public ColumnNotFoundException(Guid columnId)
        : base(
            $"Column with Id '{columnId}' is not found.",
            "board_write.column_not_found",
            404)
    {
        ColumnId = columnId;
    }
}