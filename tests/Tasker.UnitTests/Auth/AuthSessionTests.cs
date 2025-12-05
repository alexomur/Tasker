using FluentAssertions;
using Tasker.Auth.Domain.Sessions;

namespace Tasker.UnitTests.Auth;

public class AuthSessionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var session = new AuthSession(userId, createdAt);

        // Assert
        session.UserId.Should().Be(userId);
        session.CreatedAt.Should().Be(createdAt);
    }
}