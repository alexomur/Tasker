using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Boards.UpdateBoard;

public class UpdateBoardHandler : IRequestHandler<UpdateBoardCommand, Result<BaseResponseDto>>
{
    private readonly IBoardRepository _boardRepository;

    public UpdateBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BaseResponseDto>> Handle(UpdateBoardCommand? request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Result.Fail<BaseResponseDto>("Request is null.");
        }

        var boardEntity = await _boardRepository.GetByIdWithGraphAsync(request.BoardId, cancellationToken);

        if (boardEntity == null)
        {
            return Result.NotFound<BaseResponseDto>("Board not found.");
        }

        boardEntity.UpdateDetails(request.Title, request.Description);

        await _boardRepository.UpdateAsync(boardEntity, cancellationToken);

        var response = new BaseResponseDto
        {
            Message = "Board updated."
        };

        return Result.Ok(response);
    }
}