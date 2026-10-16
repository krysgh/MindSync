using FluentAssertions;
using MindSync.Domain.Exceptions;
using MindSync.Domain.ValueObjects;

namespace MindSync.Tests.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateInstance()
    {
        // Arrange
        var validEmail = "dev@mindsync.com";

        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    public static TheoryData<string> GetInvalidEmails()
    {
        var data = new TheoryData<string>
        {
            "usuario.com",
            "usuario@",
            "@dominio.com"
        };
        return data;
    }

    [Theory]
    [MemberData(nameof(GetInvalidEmails))]
    public void Constructor_WithInvalidEmail_ShouldThrowDomainException(string invalidEmail)
    {
        // Act
        Action act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O formato do e-mail fornecido é inválido.");
    }
}
