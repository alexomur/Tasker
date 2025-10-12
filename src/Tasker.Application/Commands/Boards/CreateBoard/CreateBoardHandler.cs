using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs;
using Tasker.Application.DTOs.Boards;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Boards.CreateBoard;

public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, Result<BoardDto?>>
{
    private readonly IBoardRepository _boardRepository;

    public CreateBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BoardDto?>> Handle(CreateBoardCommand? request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Result.Fail<BoardDto?>("Request is null.");
        }

        var board = new Board(request.Title, request.Description);

        var savedBoard = await _boardRepository.AddAsync(board, cancellationToken);
        

        return Result.Ok<BoardDto?>(new BoardsMapper().ToDto(savedBoard));
    }
}