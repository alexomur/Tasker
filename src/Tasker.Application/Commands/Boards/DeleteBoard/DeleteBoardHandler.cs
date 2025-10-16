using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Boards.DeleteBoard;

public class DeleteBoardHandler : IRequestHandler<DeleteBoardCommand, Result<BaseResponseDto>>
{
    private readonly IBoardRepository _boardRepository;

    public DeleteBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BaseResponseDto>> Handle(DeleteBoardCommand? request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Result.BadRequest<BaseResponseDto>("Request is null.");
        }

        var boardEntity = await _boardRepository.GetByIdWithGraphAsync(request.BoardId, cancellationToken);

        if (boardEntity == null)
        {
            return Result.NotFound<BaseResponseDto>("Board not found.");
        }

        await _boardRepository.DeleteAsync(boardEntity.Id, cancellationToken);

        var response = new BaseResponseDto
        {
            Message = "Board deleted."
        };

        return Result.Ok("Board deleted");
    }
}