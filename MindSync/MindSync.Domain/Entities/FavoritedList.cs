using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class FavoritedList
{
    private readonly List<FavoritedSession> _items = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<FavoritedSession> Items => _items.AsReadOnly();

    private FavoritedList() { }

    public FavoritedList(
        Guid id,
        Guid userId,
        string name,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O identificador da lista (Id) não pode ser vazio.");

        if (userId == Guid.Empty)
            throw new DomainException("O identificador do utilizador (UserId) não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da lista de favoritos não pode ser vazio.");

        Id = id;
        UserId = userId;
        Name = name;
        CreatedAt = createdAt;
    }

    public FavoritedList(Guid userId, string name)
        : this(Guid.NewGuid(), userId, name, DateTime.UtcNow)
    {
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("O novo nome da lista de favoritos não pode ser vazio.");

        Name = newName;
    }

    public void AddItem(FavoritedSession item)
    {
        if (item == null)
            throw new DomainException("O item favoritado não pode ser nulo.");

        if (_items.Any(x => x.StressSessionId == item.StressSessionId))
            throw new DomainException("Esta sessão de estresse já foi adicionada a esta lista de favoritos.");

        _items.Add(item);
    }

    public void RemoveItem(Guid favoritedSessionId)
    {
        var item = _items.FirstOrDefault(x => x.Id == favoritedSessionId);
        if (item == null)
            throw new DomainException("A sessão favoritada não foi encontrada nesta lista.");

        _items.Remove(item);
    }
}