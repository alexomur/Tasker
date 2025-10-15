using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Columns;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Queries.Columns.GetAllColumnsByBoardId;

public class GetAllColumnsHandler : IRequestHandler<GetAllColumnsCommand, Result<List<ColumnDto>>>
{
    private readonly IColumnRepository _columnRepository;

    public GetAllColumnsHandler(IColumnRepository columnRepository)
    {
        _columnRepository = columnRepository;
    }

    public async Task<Result<List<ColumnDto>>> Handle(GetAllColumnsCommand? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<List<ColumnDto>>("Request is null.");
        }

        var columnEntities = await _columnRepository.ListByBoardIdAsync(request.BoardId, cancellationToken);
        
        return Result.Ok(new ColumnsMapper().ToDtoList(columnEntities));
    }
}