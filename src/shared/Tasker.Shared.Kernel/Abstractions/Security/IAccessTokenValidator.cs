namespace Tasker.Shared.Kernel.Abstractions.Security
{
    /// <summary>
    /// Абстракция валидации access-токена и получения идентификатора пользователя.
    /// Реализация может ходить в Auth-сервис, Redis и т.п.
    /// </summary>
    public interface IAccessTokenValidator
    {
        /// <summary>
        /// Валидирует access-токен и возвращает идентификатор пользователя.
        /// В случае невалидного или просроченного токена должно быть выброшено исключение.
        /// </summary>
        /// <param name="accessToken">Строка access-токена.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Идентификатор пользователя.</returns>
        Task<Guid> ValidateAsync(string accessToken, CancellationToken ct);
    }
}