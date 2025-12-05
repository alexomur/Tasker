using FluentAssertions;
using Tasker.Auth.Domain.Events;
using Tasker.Auth.Domain.Users;

namespace Tasker.UnitTests.Auth;

public class UserTests
{
    [Fact]
    public void Register_ShouldInitializePropertiesAndRaiseUserRegisteredEvent()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var user = User.Register(
            emailRaw: "  User@example.COM ",
            displayName: "  John Doe  ",
            passwordHash: "hash",
            createdAt: createdAt);

        // Assert
        user.Email.Value.Should().Be("User@example.com");
        user.DisplayName.Should().Be("John Doe");
        user.PasswordHash.Should().Be("hash");

        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(createdAt);
        user.LastPasswordChangedAt.Should().Be(createdAt);

        user.EmailConfirmed.Should().BeFalse();
        user.IsLocked.Should().BeFalse();

        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.Single().Should().BeOfType<UserRegistered>();
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail_ResetConfirmation_AndRaiseEvent()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var user = User.Register(
            emailRaw: "old@example.com",
            displayName: "User",
            passwordHash: "hash",
            createdAt: createdAt);

        var confirmAt = createdAt.AddMinutes(5);
        user.ConfirmEmail(confirmAt);

        user.ClearDomainEvents();

        var changeAt = createdAt.AddHours(1);

        // Act
        user.ChangeEmail("new@example.com", changeAt);

        // Assert
        user.Email.Value.Should().Be("new@example.com");
        user.EmailConfirmed.Should().BeFalse();
        user.UpdatedAt.Should().Be(changeAt);

        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.Single().Should().BeOfType<UserEmailChanged>();
    }

    [Fact]
    public void LockAndUnlock_ShouldUpdateFlagsAndRaiseCorrespondingEvents()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var user = User.Register(
            emailRaw: "user@example.com",
            displayName: "User",
            passwordHash: "hash",
            createdAt: createdAt);

        user.ClearDomainEvents();

        var lockedAt = createdAt.AddMinutes(10);
        var unlockedAt = createdAt.AddHours(1);

        // Act: Lock
        user.Lock("  some reason  ", lockedAt);

        // Assert: после Lock
        user.IsLocked.Should().BeTrue();
        user.LockedAt.Should().Be(lockedAt);
        user.LockReason.Should().Be("some reason");
        user.UpdatedAt.Should().Be(lockedAt);

        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.Single().Should().BeOfType<UserLocked>();

        // Act: Unlock
        user.ClearDomainEvents();
        user.Unlock(unlockedAt);

        // Assert: после Unlock
        user.IsLocked.Should().BeFalse();
        user.LockedAt.Should().BeNull();
        user.LockReason.Should().BeNull();
        user.UpdatedAt.Should().Be(unlockedAt);

        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.Single().Should().BeOfType<UserUnlocked>();
    }
}
