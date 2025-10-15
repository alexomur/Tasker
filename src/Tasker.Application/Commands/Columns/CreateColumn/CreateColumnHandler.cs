using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Columns;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Columns.CreateColumn;

public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, Result<ColumnDto?>>
{
    private readonly IBoardRepository _boardRepository;

    public CreateColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<ColumnDto?>> Handle(CreateColumnCommand? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<ColumnDto?>("Request is null");
        }
        
        var column = await _boardRepository.AddColumnAsync(
            request.BoardId,
            request.Title,
            request.Description,
            cancellationToken);

        return Result.Ok<ColumnDto?>(new ColumnsMapper().ToDto(column));
    }
}