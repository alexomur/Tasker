using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Boards;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Boards.GetAllBoards;

public class GetAllBoardsHandler : IRequestHandler<GetAllBoardsCommand, Result<List<BoardDto>>>
{
    private readonly IBoardRepository _boardRepository;

    public GetAllBoardsHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<List<BoardDto>>> Handle(GetAllBoardsCommand? request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Result.Fail<List<BoardDto>>("Request is null.");
        }

        var boardEntities = await _boardRepository.ListAllAsync(cancellationToken);

        if (boardEntities == null)
        {
            return Result.Fail<List<BoardDto>>("Failed to load boards.");
        }

        var mapper = new BoardsMapper();

        var dtos = boardEntities
            .Select(b => mapper.ToDto(b))
            .Where(dto => dto != null)
            .ToList();

        return Result.Ok(dtos);
    }
}