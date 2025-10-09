using Microsoft.EntityFrameworkCore;
using Tasker.Core.Boards;

namespace Tasker.Data.Repositories;

public class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(TaskerDbContext db) : base(db)
    {
        
    }

    public async Task<IReadOnlyList<Card>> ListByColumnIdAsync(Guid columnId, CancellationToken ct = default)
    {
        var cards = await _db.Cards
            .AsNoTracking()
            .Where(card => EF.Property<Guid>(card, "ColumnId") == columnId)
            .ToListAsync(ct);

        return cards;
    }

    public async Task<Card?> UpdateFieldsAsync(Guid cardId, string? title = null, string? description = null, CancellationToken ct = default)
    {
        var cardEntity = await _db.Cards.FindAsync([cardId], ct);

        if (cardEntity == null)
        {
            return null;
        }

        if (title is not null)
        {
            cardEntity.Title = title;
        }

        if (description is not null)
        {
            cardEntity.Description = description;
        }

        await _db.SaveChangesAsync(ct);
        return cardEntity;
    }

    public async Task<bool> DeleteByIdAsync(Guid cardId, CancellationToken ct = default)
    {
        var cardEntity = await _db.Cards.FindAsync(new object[] { cardId }, ct);

        if (cardEntity == null)
        {
            return false;
        }

        _db.Cards.Remove(cardEntity);

        await _db.SaveChangesAsync(ct);
        return true;
    }
}