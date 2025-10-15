using Tasker.Application.DTOs.Columns;

namespace Tasker.Application.DTOs.Boards;

public sealed record BoardDto(Guid Id, string Title, string? Description, List<ColumnDto> Columns);