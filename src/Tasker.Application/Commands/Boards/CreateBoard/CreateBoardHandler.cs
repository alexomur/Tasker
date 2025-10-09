using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Boards.CreateBoard;

public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, Result<BaseResponseDto>>
{
    private readonly IBoardRepository _boardRepository;

    public CreateBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BaseResponseDto>> Handle(CreateBoardCommand? request, CancellationToken ct)
    {
        if (request == null)
        {
            return Result.Fail<BaseResponseDto>("Request is null.");
        }

        var board = new Board(request.Title, request.Description);

        var created = await _boardRepository.AddAsync(board, ct);

        var response = new BaseResponseDto()
        {
            Message = "Board created"
        };

        return Result.Ok(response);
    }
}