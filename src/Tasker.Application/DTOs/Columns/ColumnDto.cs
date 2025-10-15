using Tasker.Application.DTOs.Cards;

namespace Tasker.Application.DTOs.Columns;

public sealed record ColumnDto(Guid Id, string Title, string? Description, List<CardDto> Cards);