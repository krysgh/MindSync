using MindSync.Domain.Entities;

namespace MindSync.Domain.Interfaces;

public interface IAudioTrackRepository
{
    Task<IEnumerable<AudioTrack>> GetCandidatesAsync(string targetFrequency, CancellationToken cancellationToken = default);

    Task<IEnumerable<AudioTrack>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}