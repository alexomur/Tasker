using Tasker.Application.DTOs.Columns;

namespace Tasker.Application.DTOs.Boards;

public sealed record BoardListItemDto(Guid Id, string Title, string? Description, List<ColumnListItemDto> Columns);