using Riok.Mapperly.Abstractions;
using Tasker.Application.DTOs.Columns;
using Tasker.Core.Boards;

namespace Tasker.Application.Mappers;

[Mapper]
public partial class ColumnsMapper
{
    public partial ColumnDto ToDto(Column column);
    
    public partial List<ColumnDto> ToDtoList(IEnumerable<Column> columns);
}