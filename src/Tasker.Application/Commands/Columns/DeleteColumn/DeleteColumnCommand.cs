using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;

namespace Tasker.Application.Commands.Columns.DeleteColumn;

public record DeleteColumnCommand(Guid BoardId, Guid ColumnId) : IRequest<Result<BaseResponseDto>>;