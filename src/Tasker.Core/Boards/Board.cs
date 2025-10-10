using System;
using System.Collections.Generic;
using System.Linq;
using Tasker.Core.Extensions;

namespace Tasker.Core.Boards;

public class Board : Entity
{
    private List<Column> _columns = [];
    
    private List<BoardMember> _members = [];

    protected Board() { }

    public Board(string title, string? description = null)
    {
        Title = StringExtensions.Cleared(title);
        if (string.IsNullOrEmpty(Title))
        {
            throw new ArgumentException("Board title cannot be empty.", nameof(title));
        }

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public string Title { get; private set; } = "Title";

    public string? Description { get; private set; }

    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    public Column AddColumn(string title, string? description = null)
    {
        var normalized = StringExtensions.Cleared(title);
        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
        }

        if (_columns.Any(c => string.Equals(c.Title, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A column with title '{normalized}' already exists on this board.");
        }

        var column = new Column(normalized, description);
        _columns.Add(column);
        return column;
    }

    public Column? GetColumn(Guid id)
    {
        return _columns.FirstOrDefault(c => c.Id == id);
    }

    public Column? UpdateColumn(Guid id, string? title = null, string? description = null)
    {
        var column = GetColumn(id);
        if (column is null)
        {
            return null;
        }

        if (title is not null)
        {
            var normalized = StringExtensions.Cleared(title);
            if (string.IsNullOrEmpty(normalized))
            {
                throw new ArgumentException("Column title cannot be empty.", nameof(title));
            }

            if (_columns.Any(c => c.Id != id && string.Equals(c.Title, normalized, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A column with title '{normalized}' already exists on this board.");
            }

            column.Title = normalized;
        }

        if (description is not null)
        {
            column.Description = description;
        }

        return column;
    }

    public bool RemoveColumn(Guid id)
    {
        var col = GetColumn(id);
        if (col is null)
        {
            return false;
        }

        return _columns.Remove(col);
    }

    public Card MoveCard(Guid cardId, Guid toColumnId, int? insertIndex = null)
    {
        var sourceColumn = _columns.FirstOrDefault(c => c.Cards.Any(card => card.Id == cardId));
        if (sourceColumn is null)
        {
            throw new KeyNotFoundException($"Card with id '{cardId}' was not found in this board.");
        }

        var targetColumn = GetColumn(toColumnId);
        if (targetColumn is null)
        {
            throw new KeyNotFoundException($"Target column with id '{toColumnId}' was not found.");
        }

        if (sourceColumn == targetColumn)
        {
            if (insertIndex is null)
            {
                return sourceColumn.GetCard(cardId)!;
            }

            sourceColumn.ReorderCard(cardId, insertIndex.Value);
            return sourceColumn.GetCard(cardId)!;
        }

        var card = sourceColumn.GetCard(cardId);
        if (card is null)
        {
            throw new InvalidOperationException("Unexpected error: card not found in source column.");
        }

        var removed = sourceColumn.RemoveCard(cardId);
        if (!removed)
        {
            throw new InvalidOperationException("Failed to remove card from source column.");
        }

        if (insertIndex is null)
        {
            targetColumn.AddExistingCard(card);
            return targetColumn.Cards.Last();
        }

        targetColumn.AddExistingCard(card, insertIndex.Value);
        return targetColumn.Cards.ElementAt(insertIndex.Value);
    }

    public BoardMember AddMember(Guid userId, BoardRole role)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        }

        if (_members.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException("User is already a member of the board.");
        }

        var member = new BoardMember(userId, role);
        _members.Add(member);
        return member;
    }

    public bool RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            return false;
        }

        return _members.Remove(member);
    }

    public BoardMember? GetMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);
    }

    public bool IsUserAtLeast(Guid userId, BoardRole requiredRole)
    {
        var member = GetMember(userId);
        if (member is null)
        {
            return false;
        }

        return member.Role <= requiredRole ? true : false;
    }

    public void UpdateDetails(string title, string? description)
    {
        var normalized = StringExtensions.Cleared(title);
        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("Board title cannot be empty.", nameof(title));
        }

        Title = normalized;

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
