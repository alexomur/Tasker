using FluentAssertions;
using Tasker.Auth.Application.Users.Commands.RegisterUser;
using Tasker.Auth.Domain.Errors;
using Tasker.Auth.Domain.Users;

namespace Tasker.UnitTests.Auth;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateUser_WhenEmailNotRegistered()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var passwordService = new FakePasswordService();
        var handler = new RegisterUserHandler(userRepository, unitOfWork, passwordService);

        var command = new RegisterUserCommand(
            Email: "user@example.com",
            DisplayName: "John Doe",
            Password: "P@ssw0rd");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserId.Should().NotBe(Guid.Empty);

        userRepository.Users.Should().ContainSingle();
        var user = userRepository.Users.Single();

        user.Id.Should().Be(result.UserId);
        user.Email.Value.Should().Be("user@example.com");
        user.DisplayName.Should().Be("John Doe");
        user.PasswordHash.Should().Be("HASHED:P@ssw0rd");

        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyRegistered()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var passwordService = new FakePasswordService();
        var handler = new RegisterUserHandler(userRepository, unitOfWork, passwordService);

        var now = DateTimeOffset.UtcNow;
        var existing = User.Register(
            emailRaw: "user@example.com",
            displayName: "Existing User",
            passwordHash: "existing-hash",
            createdAt: now);

        userRepository.AddUser(existing);

        var command = new RegisterUserCommand(
            Email: "user@example.com",
            DisplayName: "John Doe",
            Password: "P@ssw0rd");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EmailAlreadyRegisteredException>();

        unitOfWork.SaveChangesCallCount.Should().Be(0);
    }
}
