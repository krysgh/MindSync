using MindSync.Domain.Entities;

namespace MindSync.Domain.Interfaces;

public interface IStressSessionRepository
{
    Task<IEnumerable<StressSession>> GetByUserIdAsync(Guid userId);
    Task<StressSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task UpdateAsync(StressSession session, CancellationToken cancellationToken = default);
    Task AddAsync(StressSession session, CancellationToken cancellationToken = default);
}