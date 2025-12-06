using Tasker.BoardWrite.Application.Boards.Templates;

namespace Tasker.BoardWrite.Api.Controllers.Boards.Models;

public sealed record BoardTemplateDto(
    string Code,
    string Name,
    string Description,
    string Category,
    IReadOnlyCollection<string> Tags
)
{
    public static BoardTemplateDto FromDomain(BoardTemplateInfo info) =>
        new BoardTemplateDto(info.Code, info.Name, info.Description, info.Category, info.Tags);
}