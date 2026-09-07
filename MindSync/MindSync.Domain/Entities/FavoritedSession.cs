using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class FavoritedSession
{
    public Guid Id { get; private set; }
    public Guid FavoritedListId { get; private set; }
    public Guid StressSessionId { get; private set; }
    public string CustomName { get; private set; } = null!;
    public DateTime FavoritedAt { get; private set; }

    private FavoritedSession() { }

    public FavoritedSession(
        Guid id,
        Guid favoritedListId,
        Guid stressSessionId,
        string customName,
        DateTime favoritedAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O identificador do item favoritado (Id) não pode ser vazio.");

        if (favoritedListId == Guid.Empty)
            throw new DomainException("O identificador da lista de favoritos (FavoriteListId) não pode ser vazio.");

        if (stressSessionId == Guid.Empty)
            throw new DomainException("O identificador da sessão de estresse (StressSessionId) não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(customName))
            throw new DomainException("O nome customizado da sessão favoritada não pode ser vazio.");

        Id = id;
        FavoritedListId = favoritedListId;
        StressSessionId = stressSessionId;
        CustomName = customName;
        FavoritedAt = favoritedAt;
    }

    public FavoritedSession(Guid favoritedListId, Guid stressSessionId, string customName)
    : this(Guid.NewGuid(), favoritedListId, stressSessionId, customName, DateTime.UtcNow)
    {
    }

    public void UpdateCustomName(string newCustomName)
    {
        if (string.IsNullOrWhiteSpace(newCustomName))
            throw new DomainException("O novo nome customizado da sessão não pode ser vazio.");

        CustomName = newCustomName;
    }
}