using Tasker.Shared.Kernel.Errors;

namespace Tasker.BoardWrite.Domain.Errors;

/// <summary>
/// Исключение, выбрасываемое при попытке обращения к несуществующей карточке.
/// </summary>
public sealed class CardNotFoundException : AppException
{
    public Guid CardId { get; }

    public CardNotFoundException(Guid cardId) : base($"Card with Id '{cardId}' is not found.", "board_write.card_not_found", 404)
    {
        CardId = cardId;
    }
}