namespace Tasker.Shared.Kernel.Abstractions;

/// <summary>
/// Представляет текущего пользователя в контексте выполняющегося запроса.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Признак того, что пользователь аутентифицирован.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Идентификатор текущего пользователя или null, если пользователь анонимный.
    /// </summary>
    Guid? UserId { get; }
}