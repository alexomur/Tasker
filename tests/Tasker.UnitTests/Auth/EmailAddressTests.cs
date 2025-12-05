using FluentAssertions;
using Tasker.Auth.Domain.ValueObjects;

namespace Tasker.UnitTests.Auth;

public class EmailAddressTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeDomainAndTrim()
    {
        // Arrange
        const string raw = "  User.Name+tag@ExAmPle.COM  ";

        // Act
        var email = EmailAddress.Create(raw);

        // Assert
        email.Value.Should().Be("User.Name+tag@example.com");
        email.ToString().Should().Be("User.Name+tag@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string raw)
    {
        // Act
        Action act = () => EmailAddress.Create(raw);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ShouldReturnFalseAndDefaultValue()
    {
        // Act
        var success = EmailAddress.TryCreate("invalid", out var email);

        // Assert
        success.Should().BeFalse();
        email.Should().Be(default(EmailAddress));
    }
}