using Tasker.BoardRead.Domain.UserViews;

namespace Tasker.BoardRead.Application.Users.Abstractions;

public interface IUserReadService
{
    /// <summary>
    /// Вернуть пользователей по их идентификаторам.
    /// </summary>
    Task<IReadOnlyCollection<UserView>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}