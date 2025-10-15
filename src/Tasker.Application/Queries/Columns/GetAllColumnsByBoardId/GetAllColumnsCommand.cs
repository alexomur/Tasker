using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Columns;

namespace Tasker.Application.Queries.Columns.GetAllColumnsByBoardId;

public record GetAllColumnsCommand(Guid BoardId) : IRequest<Result<List<ColumnDto>>>;