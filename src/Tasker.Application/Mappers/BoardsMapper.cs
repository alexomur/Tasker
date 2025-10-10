using Riok.Mapperly.Abstractions;
using Tasker.Application.DTOs.Boards;
using Tasker.Core.Boards;

namespace Tasker.Application.Mappers;

[Mapper]
public partial class BoardsMapper
{
    public partial BoardDto ToDto(Board board);
    
    public partial List<BoardDto> ToDtoList(IEnumerable<Board> boards);
}