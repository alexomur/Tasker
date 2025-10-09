using System.Security.Cryptography;

namespace Tasker.Core.Users;

public class User : Entity
{
    private string _email = null!;
    private string _username = null!;
    private string? _displayName;
    private string _passwordHash = null!;
    private DateTimeOffset _createdAt;
    private DateTimeOffset? _lastLoginAt;
    private bool _isActive;

    public const int MaxEmailLength = 256;
    public const int MaxUsernameLength = 50;
    public const int MinPasswordLength = 8;
    public const int MaxDisplayNameLength = 200;

    protected User() { }

    public User(string email, string username, string password, string? displayName = null)
    {
        SetEmail(email);
        SetUsername(username);
        SetPassword(password);
        SetDisplayName(displayName);
        _createdAt = DateTimeOffset.UtcNow;
        _isActive = true;
    }

    public string Email
    {
        get => _email;
        private set => _email = value;
    }

    public string Username
    {
        get => _username;
        private set => _username = value;
    }

    public string? DisplayName
    {
        get => _displayName;
        private set => _displayName = value;
    }

    public DateTimeOffset CreatedAt => _createdAt;

    public DateTimeOffset? LastLoginAt => _lastLoginAt;

    public bool IsActive => _isActive;

    public void ChangeEmail(string email)
    {
        SetEmail(email);
    }

    public void ChangeUsername(string username)
    {
        SetUsername(username);
    }

    public void SetDisplayName(string? displayName)
    {
        var normalized = (displayName ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            _displayName = null;
        }
        else
        {
            if (normalized.Length > MaxDisplayNameLength)
            {
                throw new ArgumentException($"Display name is too long (max {MaxDisplayNameLength}).", nameof(displayName));
            }

            _displayName = normalized;
        }
    }

    public void SetPassword(string password)
    {
        var pass = (password ?? string.Empty).Trim();

        if (pass.Length < MinPasswordLength)
        {
            throw new ArgumentException($"Password must be at least {MinPasswordLength} characters long.", nameof(password));
        }

        _passwordHash = CreateHash(pass);
    }

    public bool VerifyPassword(string password)
    {
        var pass = (password ?? string.Empty);
        return VerifyHash(_passwordHash, pass);
    }

    public void MarkLastLogin(DateTimeOffset when)
    {
        _lastLoginAt = when;
    }

    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
    }

    private void SetEmail(string email)
    {
        var e = (email ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(e))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (e.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email is too long (max {MaxEmailLength}).", nameof(email));
        }

        if (!e.Contains('@') || e.LastIndexOf('.') < e.IndexOf('@'))
        {
            throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        _email = e.ToLowerInvariant();
    }

    private void SetUsername(string username)
    {
        var u = (username ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(u))
        {
            throw new ArgumentException("Username cannot be empty.", nameof(username));
        }

        if (u.Length > MaxUsernameLength)
        {
            throw new ArgumentException($"Username is too long (max {MaxUsernameLength}).", nameof(username));
        }

        if (u.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')))
        {
            throw new ArgumentException("Username contains invalid characters. Allowed: letters, digits, underscore, hyphen.", nameof(username));
        }

        _username = u;
    }

    private static string CreateHash(string password)
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        const int iterations = 100_000;
        const int hashLength = 32;

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(hashLength);

        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(hash);

        return $"{iterations}.{saltBase64}.{hashBase64}";
    }

    private static bool VerifyHash(string storedHash, string password)
    {
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var actualHash = pbkdf2.GetBytes(expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
