using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Tasker.Auth.Application.Users.Commands.LoginUser;
using Tasker.Auth.Domain.Errors;
using Tasker.Auth.Domain.Users;

namespace Tasker.UnitTests.Auth;

public class LoginUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordService = new FakePasswordService
        {
            VerifyResult = PasswordVerificationResult.Success
        };
        var authSessionStore = new FakeAuthSessionStore
        {
            TokenToReturn = "session-123"
        };
        var unitOfWork = new FakeUnitOfWork();

        var handler = new LoginUserHandler(userRepository, passwordService, authSessionStore, unitOfWork);

        var now = DateTimeOffset.UtcNow;
        var user = User.Register(
            emailRaw: "user@example.com",
            displayName: "John Doe",
            passwordHash: "initial-hash",
            createdAt: now);

        // Обновляем хэш, чтобы был какой-то "боевой"
        user.ChangePasswordHash("HASHED:P@ssw0rd", now);
        userRepository.AddUser(user);

        var ttl = TimeSpan.FromMinutes(15);
        var command = new LoginUserCommand(
            Email: "user@example.com",
            Password: "P@ssw0rd",
            Ttl: ttl);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(user.Id);
        result.AccessToken.Should().Be("session-123");
        result.ExpiresInSeconds.Should().Be((long)ttl.TotalSeconds);

        authSessionStore.Created.Should().ContainSingle();
        var created = authSessionStore.Created.Single();
        created.UserId.Should().Be(user.Id);
        created.Ttl.Should().Be(ttl);

        // Сохраняем событие успешного логина
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var handler = new LoginUserHandler(
            new FakeUserRepository(),
            new FakePasswordService(),
            new FakeAuthSessionStore(),
            new FakeUnitOfWork());

        var command = new LoginUserCommand(
            Email: "unknown@example.com",
            Password: "whatever",
            Ttl: TimeSpan.FromMinutes(5));

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserLocked()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordService = new FakePasswordService
        {
            VerifyResult = PasswordVerificationResult.Success
        };
        var authSessionStore = new FakeAuthSessionStore();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new LoginUserHandler(userRepository, passwordService, authSessionStore, unitOfWork);

        var now = DateTimeOffset.UtcNow;
        var user = User.Register(
            emailRaw: "locked@example.com",
            displayName: "Locked User",
            passwordHash: "hash",
            createdAt: now);

        user.Lock("Too many attempts", now);
        userRepository.AddUser(user);

        var command = new LoginUserCommand(
            Email: "locked@example.com",
            Password: "P@ssw0rd",
            Ttl: TimeSpan.FromMinutes(5));

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UserLockedException>();
        authSessionStore.Created.Should().BeEmpty();
    }
}
