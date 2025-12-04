using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Tasker.Shared.Kernel.Abstractions.Security;

namespace Tasker.BoardRead.Api.Security
{
    /// <summary>
    /// Обработчик аутентификации по заголовку Authorization: Bearer {accessToken}.
    /// Делегирует проверку токена в <see cref="IAccessTokenValidator"/>.
    /// </summary>
    public sealed class AccessTokenAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "AccessToken";

        private readonly IAccessTokenValidator _accessTokenValidator;

        public AccessTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IAccessTokenValidator accessTokenValidator)
            : base(options, logger, encoder)
        {
            _accessTokenValidator = accessTokenValidator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return AuthenticateResult.NoResult();
            }

            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail(
                    "Неверный формат заголовка Authorization. Ожидается 'Bearer {token}'.");
            }

            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AuthenticateResult.Fail("Токен авторизации отсутствует.");
            }

            try
            {
                var userId = await _accessTokenValidator.ValidateAsync(
                    accessToken,
                    Context.RequestAborted);

                var claims = new[]
                {
                    new Claim("userId", userId.ToString())
                };

                var identity = new ClaimsIdentity(claims, SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Ошибка валидации access-токена.");
                return AuthenticateResult.Fail("Невалидный или просроченный access-токен.");
            }
        }
    }
}
