using System.Text.Json;
using StackExchange.Redis;
using Tasker.Shared.Kernel.Abstractions.Security;

namespace Tasker.BoardWrite.Infrastructure.Security
{
    /// <summary>
    /// Валидатор access-токена на основе Redis-сессий Auth-сервиса.
    /// Ожидает, что токен совпадает с ключом auth:sessions:{token},
    /// а значение содержит JSON с полем UserId.
    /// </summary>
    public sealed class RedisAccessTokenValidator : IAccessTokenValidator
    {
        private const string SessionKeyPrefix = "auth:sessions:";
        private readonly IDatabase _database;

        public RedisAccessTokenValidator(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<Guid> ValidateAsync(string accessToken, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new UnauthorizedAccessException("Токен авторизации не задан.");
            }

            var key = SessionKeyPrefix + accessToken;
            var redisValue = await _database.StringGetAsync(key);

            if (redisValue.IsNullOrEmpty)
            {
                throw new UnauthorizedAccessException("Сессия не найдена или истекла.");
            }

            AuthSessionPayload? session;

            try
            {
                session = JsonSerializer.Deserialize<AuthSessionPayload>(redisValue!);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Не удалось прочитать данные сессии.", ex);
            }

            if (session is null || session.UserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("В сессии не найден идентификатор пользователя.");
            }

            return session.UserId;
        }

        /// <summary>
        /// Локальный DTO для десериализации JSON из Redis.
        /// Должен совпадать по именам свойств с AuthSession.
        /// </summary>
        private sealed class AuthSessionPayload
        {
            public Guid UserId { get; set; }

            public DateTimeOffset CreatedAt { get; set; }
        }
    }
}
