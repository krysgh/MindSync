using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;
using MindSync.Domain.Interfaces;

namespace MindSync.Infrastructure.Services;

public class AudioMixComposer : IAudioMixComposer
{
    private const decimal BinauralVolume = 1.0m;
    private const decimal TextureVolume = 0.6m;

    public IReadOnlyCollection<AudioMixSelection> Compose(
        int stressLevel,
        string targetFrequency,
        IEnumerable<AudioTrack> candidateTracks)
    {
        var tracks = candidateTracks.ToList();

        var binaural = tracks.FirstOrDefault(t =>
            t.Role == "Binaural" && t.TargetFrequency == targetFrequency) ??
                throw new DomainException($"Nenhuma faixa binaural encontrada para a frequência '{targetFrequency}'.");

        var textureCandidates = tracks
            .Where(t => t.Role == "Texture" && t.MatchesStressLevel(stressLevel))
            .ToList();

        var texture = textureCandidates.Count > 0
            ? textureCandidates[Random.Shared.Next(textureCandidates.Count)]
            : null;

        var selections = new List<AudioMixSelection> { new(binaural.Id, BinauralVolume) };

        if (texture is not null)
            selections.Add(new AudioMixSelection(texture.Id, TextureVolume));

        return selections;
    }
}