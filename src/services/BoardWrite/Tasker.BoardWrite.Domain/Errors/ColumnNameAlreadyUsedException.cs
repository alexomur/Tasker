using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors;

public class ColumnNameAlreadyUsedException : AppException
{
    public string ColumnName { get; }
    
    public ColumnNameAlreadyUsedException(string columnName) : base($"Column with name '{columnName}' already exists", "board_write.column_name_used", 409)
    {
        ColumnName = columnName;
    }
}