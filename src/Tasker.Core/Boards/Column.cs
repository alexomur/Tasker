namespace Tasker.Core.Boards;

public class Column : Entity
{
    private string _title = null!;
    private string? _description;
    private List<Card> _cards = [];

    protected Column() { }

    public Column(string title, string? description = null)
    {
        Title = title;
        Description = description;
    }

    public string Title
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

    public Card AddCard(string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
        }

        var card = new Card
        {
            Title = title,
            Description = description
        };

        _cards.Add(card);
        return card;
    }

    public Card AddExistingCard(Card card, int? index = null)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card));
        }

        if (index is null)
        {
            _cards.Add(card);
            return card;
        }

        if (index < 0 || index > _cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cards.Insert(index.Value, card);
        return card;
    }

    public Card? GetCard(Guid id) =>
        _cards.FirstOrDefault(c => c.Id == id);

    public Card? UpdateCard(Guid id, string? title = null, string? description = null)
    {
        var card = GetCard(id);
        if (card is null) return null;

        if (title is not null)
        {
            card.Title = title;
        }
        if (description is not null)
        {
            card.Description = description;
        }

        return card;
    }

    public bool RemoveCard(Guid id)
    {
        var card = GetCard(id);
        return card is not null && _cards.Remove(card);
    }

    public void ReorderCard(Guid id, int newIndex)
    {
        var card = _cards.FirstOrDefault(c => c.Id == id)
                   ?? throw new KeyNotFoundException($"Card with id '{id}' was not found.");

        if (newIndex < 0 || newIndex > _cards.Count - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex));
        }

        _cards.Remove(card);
        var insertAt = Math.Min(newIndex, _cards.Count);
        _cards.Insert(insertAt, card);
    }
}
