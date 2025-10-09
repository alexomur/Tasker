using System.Security.Claims;
using Tasker.Api.Interfaces;

namespace Tasker.Api;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.User?.Identity?.IsAuthenticated != true)
            {
                return Guid.Empty;
            }

            var claim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return Guid.Empty;
            }

            if (Guid.TryParse(claim.Value, out var id))
            {
                return id;
            }

            return Guid.Empty;
        }
    }

    public bool IsAuthenticated => UserId != Guid.Empty;

    public string? Username
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            return ctx?.User?.Identity?.Name;
        }
    }
}