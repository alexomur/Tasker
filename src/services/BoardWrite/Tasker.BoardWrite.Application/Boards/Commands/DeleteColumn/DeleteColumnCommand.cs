using MediatR;

namespace Tasker.BoardWrite.Application.Boards.Commands.DeleteColumn;

public sealed record DeleteColumnCommand(Guid BoardId, Guid ColumnId) : IRequest;