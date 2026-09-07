using MindSync.Domain.Entities;

namespace MindSync.Domain.Interfaces;

public interface IFavoritedListRepository
{
    Task AddAsync(FavoritedList list, CancellationToken cancellationToken = default);
    Task<FavoritedList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FavoritedList>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FavoritedSession?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(FavoritedList list, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddSessionAsync(FavoritedSession favoritedSession, CancellationToken cancellationToken = default);
    Task UpdateSessionAsync(FavoritedSession favoritedSession, CancellationToken cancellationToken = default);
    Task RemoveSessionAsync(Guid favoritedSessionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndUserIdAsync(string name, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCustomNameAndListIdAsync(
        string customName,
        Guid favoritedListId,
        Guid? excludingSessionId = null,
        CancellationToken cancellationToken = default);
}