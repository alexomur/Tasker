using MediatR;
using Tasker.Application.Common;
using Tasker.Application.DTOs.Columns;
using Tasker.Application.Mappers;
using Tasker.Core.Boards;

namespace Tasker.Application.Commands.Columns.CreateColumn;

public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, Result<ColumnDto?>>
{
    private readonly IColumnRepository _columnRepository;

    public CreateColumnHandler(IColumnRepository columnRepository)
    {
        _columnRepository = columnRepository;
    }

    public async Task<Result<ColumnDto?>> Handle(CreateColumnCommand? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<ColumnDto?>("Request is null");
        }
        
        var column = new Column(request.Title, request.Description);
        
        var savedColumn = await _columnRepository.AddAsync(column, cancellationToken);

        return Result.Ok<ColumnDto?>(new ColumnsMapper().ToDto(savedColumn));
    }
}