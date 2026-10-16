using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;

namespace MindSync.Tests.UnitTests.Entities;

public class FavoritedListTests
{
    public static TheoryData<string> GetInvalidNames()
    {
        return new TheoryData<string>
        {
            "",
            "   "
        };
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateFavoritedList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "Minhas Sessões Foco";

        // Act
        var list = new FavoritedList(userId, name);

        // Assert
        list.Id.Should().NotBeEmpty();
        list.UserId.Should().Be(userId);
        list.Name.Should().Be(name);
        list.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        list.Items.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new FavoritedList(Guid.Empty, "Nome Válido");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O identificador do utilizador (UserId) não pode ser vazio.");
    }

    [Theory]
    [MemberData(nameof(GetInvalidNames))]
    public void Constructor_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Act
        Action act = () => new FavoritedList(Guid.NewGuid(), invalidName);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O nome da lista de favoritos não pode ser vazio.");
    }

    [Fact]
    public void AddItem_WithValidSession_ShouldAddSessionToList()
    {
        // Arrange
        var list = new FavoritedList(Guid.NewGuid(), "Relaxamento");
        var session = new FavoritedSession(list.Id, Guid.NewGuid(), "Sessão Noturna");

        // Act
        list.AddItem(session);

        // Assert
        list.Items.Should().HaveCount(1);
        list.Items.Should().Contain(session);
    }

    [Fact]
    public void AddItem_WithDuplicateSession_ShouldThrowDomainException()
    {
        // Arrange
        var list = new FavoritedList(Guid.NewGuid(), "Relaxamento");
        var stressSessionId = Guid.NewGuid();

        var session1 = new FavoritedSession(list.Id, stressSessionId, "Sessão A");
        var session2 = new FavoritedSession(list.Id, stressSessionId, "Sessão B");

        list.AddItem(session1);

        // Act
        Action act = () => list.AddItem(session2);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Esta sessão de estresse já foi adicionada a esta lista de favoritos.");
    }

    [Fact]
    public void RemoveItem_WithExistingSession_ShouldRemoveSessionFromList()
    {
        // Arrange
        var list = new FavoritedList(Guid.NewGuid(), "Minha Lista");
        var session = new FavoritedSession(list.Id, Guid.NewGuid(), "Sessão Para Remover");
        list.AddItem(session);

        // Act
        list.RemoveItem(session.Id);

        // Assert
        list.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WithNonExistingSession_ShouldThrowDomainException()
    {
        // Arrange
        var list = new FavoritedList(Guid.NewGuid(), "Minha Lista");

        // Act
        Action act = () => list.RemoveItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("A sessão favoritada não foi encontrada nesta lista.");
    }
}