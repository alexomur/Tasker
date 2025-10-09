using Tasker.Core.Common;

namespace Tasker.Core.Boards;

public class Card : Entity
{
    private string _title = null!;
    private string? _description;
    
    public required string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Card Title cannot be null, empty or whitespace.", nameof(value));
            }
            _title = value.Trim();
        }
    }

    public string? Description
    {
        get => _description;
        set => _description = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}