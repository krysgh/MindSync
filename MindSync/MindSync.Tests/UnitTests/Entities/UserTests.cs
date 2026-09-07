using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.ValueObjects;

namespace MindSync.Tests.UnitTests.Entities;

public class UserTests
{
    private static readonly Email ValidEmailVo = new("parceiro@mindsync.com");

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "Lucas";
        var lastName = "Silva";
        var passwordHash = "hash_seguro_bcrypt";
        var createdAt = DateTime.UtcNow;

        // Act
        var user = new User(id, firstName, lastName, ValidEmailVo, passwordHash, createdAt);

        // Assert
        user.Id.Should().Be(id);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(ValidEmailVo);
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new User(Guid.Empty, "Lucas", "Silva", ValidEmailVo, "hash", DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O Id do utilizador não pode ser vazio.");
    }

    public static TheoryData<string> GetInvalidNames()
    {
        var data = new TheoryData<string>
        {
            "",
            "   "
        };
        return data;
    }

    [Theory]
    [MemberData(nameof(GetInvalidNames))]
    public void Constructor_WithInvalidFirstName_ShouldThrowDomainException(string invalidFirstName)
    {
        // Act
        Action act = () => new User(Guid.NewGuid(), invalidFirstName, "Silva", ValidEmailVo, "hash", DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O nome do utilizador não pode ser vazio.");
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new User(Guid.NewGuid(), "Lucas", "Silva", null!, "hash", DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O e-mail não pode ser vazio.");
    }
}
