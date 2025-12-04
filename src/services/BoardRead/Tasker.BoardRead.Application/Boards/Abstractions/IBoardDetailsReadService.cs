using Tasker.BoardRead.Application.Boards.Views;

namespace Tasker.BoardRead.Application.Boards.Abstractions;

/// <summary>
/// Сервис чтения детальной информации о доске для BoardRead.
/// Инкапсулирует логику: Cassandra → (fallback) MySQL.
/// </summary>
public interface IBoardDetailsReadService
{
    /// <summary>
    /// Получить детальное представление доски.
    /// Возвращает null, если доска не найдена или нет доступа.
    /// </summary>
    Task<BoardDetailsView?> GetBoardAsync(Guid boardId, CancellationToken cancellationToken = default);
}