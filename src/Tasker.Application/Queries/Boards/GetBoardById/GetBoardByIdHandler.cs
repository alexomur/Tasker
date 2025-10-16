using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Boards;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Queries.Boards.GetBoardById;

public class GetBoardByIdHandler : IRequestHandler<GetBoardByIdCommand, Result<BoardDto?>>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardByIdHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }
    
    public async Task<Result<BoardDto?>> Handle(GetBoardByIdCommand? request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Result.BadRequest<BoardDto?>("Request is null.");
        }

        var board = await _boardRepository.GetByIdWithGraphNoTrackingAsync(request.BoardId, cancellationToken);

        if (board == null)
        {
            return Result.NotFound<BoardDto?>("Board not found.");
        }

        return Result.Ok<BoardDto?>(new BoardsMapper().ToDto(board));
    }
}