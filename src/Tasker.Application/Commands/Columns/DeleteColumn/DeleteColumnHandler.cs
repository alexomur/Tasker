using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Columns.DeleteColumn;

public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Result<BaseResponseDto>>
{
    private readonly IBoardRepository _boardRepository;

    public DeleteColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BaseResponseDto>> Handle(DeleteColumnCommand? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.BadRequest<BaseResponseDto>("Request is null");
        }

        var board = await _boardRepository.GetByIdAsync(request.BoardId, cancellationToken);
        if (board is null)
        {
            return Result.NotFound<BaseResponseDto>("Board not found");
        }
        
        if (!await _boardRepository.RemoveColumnAsync(request.BoardId, request.ColumnId, cancellationToken))
        {
            return Result.NotFound<BaseResponseDto>("Column not found");
        }

        return Result.Ok("Column deleted");
    }
}