using MindSync.Domain.Entities;

namespace MindSync.Domain.Interfaces;

public interface IAudioMixComposer
{
    IReadOnlyCollection<AudioMixSelection> Compose(
        int stressLevel,
        string targetFrequency,
        IEnumerable<AudioTrack> candidateTracks);
}

public record AudioMixSelection(Guid AudioTrackId, decimal Volume);