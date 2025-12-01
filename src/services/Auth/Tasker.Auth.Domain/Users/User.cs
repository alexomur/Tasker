using Tasker.Auth.Domain.Events;
using Tasker.Auth.Domain.ValueObjects;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Domain.Users;

public class User : Entity
{
    public Guid Id { get; private set; }

    public EmailAddress Email { get; private set; } = default!;
    
    public string DisplayName { get; private set; } = null!;
    
    public string PasswordHash { get; private set; } = null!;
    
    public bool EmailConfirmed { get; private set; }
    
    public bool IsLocked { get; private set; }
    
    public DateTimeOffset? LockedAt { get; private set; }
    
    public string? LockReason { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset UpdatedAt { get; private set; }
    
    public DateTimeOffset? LastPasswordChangedAt { get; private set; }
    
    private User() { }

    private User(Guid id, EmailAddress email, string displayName, string passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        LastPasswordChangedAt = createdAt;
    }

    public static User Register(string emailRaw, string displayName, string passwordHash, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }

        if (displayName.Length > 64)
        {
            throw new ArgumentException("Display name cannot be longer than 64 characters.", nameof(displayName));
        }
        
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        }
        
        var email = EmailAddress.Create(emailRaw);
        var user = new User(Guid.NewGuid(), email, displayName.Trim(), passwordHash, createdAt);
        user.AddEvent(new UserRegistered(user.Id, email.Value, displayName.Trim(), createdAt));
        return user;
    }

    public void ConfirmEmail(DateTimeOffset confirmAt)
    {
        if (EmailConfirmed)
        {
            return;
        }

        EmailConfirmed = true;
        UpdatedAt = confirmAt;
        AddEvent(new UserEmailConfirmed(Id, confirmAt));
    }

    public void ChangeDisplayName(string newDisplayNameRaw, DateTimeOffset changeAt)
    {
        var newDisplayName = newDisplayNameRaw.Trim();
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(newDisplayNameRaw));
        }

        if (newDisplayName.Length > 64)
        {
            throw new ArgumentException("Display name cannot be longer than 64 characters.", nameof(newDisplayNameRaw));
        }

        if (newDisplayName == DisplayName)
        {
            return;
        }
        
        DisplayName = newDisplayName;
        UpdatedAt = changeAt;
        AddEvent(new UserDisplayNameChanged(Id, newDisplayName, changeAt));
    }

    public void ChangeEmail(string newEmailRaw, DateTimeOffset changeAt)
    {
        var newEmail = EmailAddress.Create(newEmailRaw);
        if (newEmail.Value == Email.Value)
        {
            return;
        }
        
        Email = newEmail;
        EmailConfirmed = false;
        UpdatedAt = changeAt;
        AddEvent(new UserEmailChanged(Id, newEmail.Value, changeAt));
    }
    
    public void ChangePasswordHash(string newPasswordHash, DateTimeOffset changeAt)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.");
        }

        if (newPasswordHash == PasswordHash)
        {
            return;
        }

        PasswordHash = newPasswordHash;
        LastPasswordChangedAt = changeAt;
        UpdatedAt = changeAt;
        AddEvent(new UserPasswordChanged(Id, changeAt));
    }
    
    public void Lock(string reason, DateTimeOffset lockedAt)
    {
        if (IsLocked)
        {
            return;
        }

        IsLocked = true;
        LockedAt = lockedAt;
        LockReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        UpdatedAt = lockedAt;
        AddEvent(new UserLocked(Id, LockReason, lockedAt));
    }

    public void Unlock(DateTimeOffset unlockedAt)
    {
        if (!IsLocked)
        {
            return;
        }

        IsLocked = false;
        LockedAt = null;
        LockReason = null;
        UpdatedAt = unlockedAt;
        AddEvent(new UserUnlocked(Id, unlockedAt));
    }
}