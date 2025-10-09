using Tasker.Core.Common;
using Tasker.Core.Extensions;

namespace Tasker.Core.Boards;

public class Board : Entity
{
    private List<Column> _columns = [];

    public Board(string title, string? description = null)
    {
        Title = StringExtensions.Cleared(title);
        if (string.IsNullOrEmpty(Title))
        {
            throw new ArgumentException("Board title cannot be empty.");
        }
        Description = description;
    }
    
    public string Title { get; private set; }
    
    public string? Description { get; private set; }

    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();

    public void AddColumn(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
        }
        
        _columns.Add(new Column
        {
            Title = title,
        });
    }
}