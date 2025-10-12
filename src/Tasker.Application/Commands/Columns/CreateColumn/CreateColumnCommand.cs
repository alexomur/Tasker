using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Columns;

namespace Tasker.Application.Commands.Columns.CreateColumn;

public record CreateColumnCommand(string Title, string? Description) : IRequest<Result<ColumnDto>>;