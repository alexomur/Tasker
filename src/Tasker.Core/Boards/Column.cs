using Tasker.Core.Common;

namespace Tasker.Core.Boards;

public class Column : Entity
{
    private string _title = null!;
    private string? _description;
    private List<Card> _cards = [];

    public required string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Column Title cannot be null, empty or whitespace.", nameof(value));
            }
            _title = value.Trim();
        }
    }

    public string? Description
    {
        get => _description;
        set => _description = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    public void AddCard(string title, string? description = null)
    {
        _cards.Add(new Card
        {
            Title = title,
            Description = description
        });
    }

    public Card? GetCard(Guid id)
    {
        return _cards.FirstOrDefault(c => c.Id == id);
    }

    public Card? UpdateCard(Guid id, string? title = null, string? description = null)
    {
        Card? card = GetCard(id);
        if (card is null)
            return null;

        if (title is not null)
            card.Title = title;
        
        if (description is not null)
            card.Description = description;

        return card;
    }

    public bool RemoveCard(Guid id)
    {
        Card? card = GetCard(id);

        return card is not null && _cards.Remove(card);
    }

    public void ReorderCard(Guid id, int newIndex)
    {
        Card card = _cards.FirstOrDefault(c => c.Id == id)
                   ?? throw new KeyNotFoundException($"Card with id '{id}' was not found.");

        if (newIndex < 0 || newIndex > _cards.Count - 1)
            throw new ArgumentOutOfRangeException(nameof(newIndex));

        _cards.Remove(card);
        _cards.Insert(newIndex, card);
    }
}