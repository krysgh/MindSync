using MindSync.Domain.Entities;

namespace MindSync.Domain.Interfaces;

public interface ISessionAudioMixRepository
{
    Task<SessionAudioMix?> GetByStressSessionIdAsync(Guid stressSessionId, CancellationToken cancellationToken = default);
    Task AddAsync(SessionAudioMix mix, CancellationToken cancellationToken = default);
}