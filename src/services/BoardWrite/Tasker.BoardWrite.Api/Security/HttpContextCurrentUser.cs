using System.Security.Claims;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Api.Security
{
    /// <summary>
    /// Реализация <see cref="ICurrentUser"/> на основе <see cref="HttpContext"/>.
    /// </summary>
    public sealed class HttpContextCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity is null)
                {
                    return false;
                }

                return user.Identity.IsAuthenticated;
            }
        }

        public Guid? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity is not { IsAuthenticated: true })
                {
                    return null;
                }

                // Клейм с идентификатором пользователя.
                // Подстрой под то, как ты будешь создавать клеймы в обработчике аутентификации.
                var rawUserId =
                    user.FindFirst("userId")?.Value ??
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    user.FindFirst("sub")?.Value;

                if (rawUserId is null)
                {
                    return null;
                }

                if (Guid.TryParse(rawUserId, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }
    }
}