using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.SessionAudioMixes.Commands.GetOrCreateSessionAudioMix;

public class GetOrCreateSessionAudioMixCommandHandler(
    ISessionAudioMixRepository mixRepository,
    IStressSessionRepository stressSessionRepository,
    IAudioTrackRepository audioTrackRepository,
    IAudioMixComposer audioMixComposer,
    IAudioFileUrlResolver audioFileUrlResolver,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetOrCreateSessionAudioMixCommandRequest, SessionAudioMixResponse>
{
    public async Task<SessionAudioMixResponse> Handle(
        GetOrCreateSessionAudioMixCommandRequest request,
        CancellationToken cancellationToken)
    {
        var existingMix = await mixRepository.GetByStressSessionIdAsync(request.StressSessionId, cancellationToken);

        if (existingMix is not null)
            return await BuildResponseAsync(existingMix, cancellationToken);

        var session = await stressSessionRepository.GetSessionByIdAsync(request.StressSessionId, cancellationToken);

        if (session is null || session.UserId != currentUserService.UserId)
            throw new NotFoundException($"Sessão de estresse com o ID '{request.StressSessionId}' não foi encontrada.");

        var candidates = (await audioTrackRepository.GetCandidatesAsync(session.TargetFrequency, cancellationToken)).ToList();
        var selections = audioMixComposer.Compose((int)session.StressLevelBefore, session.TargetFrequency, candidates);

        var mix = new SessionAudioMix(request.StressSessionId);
        foreach (var selection in selections)
            mix.AddLayer(selection.AudioTrackId, selection.Volume);

        try
        {
            await mixRepository.AddAsync(mix, cancellationToken);
        }
        catch (Exception)
        {
            var raceMix = await mixRepository.GetByStressSessionIdAsync(request.StressSessionId, cancellationToken);

            if (raceMix is null)
                throw;

            return await BuildResponseAsync(raceMix, cancellationToken);
        }

        return await BuildResponseAsync(mix, cancellationToken);
    }

    private async Task<SessionAudioMixResponse> BuildResponseAsync(
        SessionAudioMix mix,
        CancellationToken cancellationToken)
    {
        var trackIds = mix.Layers.Select(l => l.AudioTrackId);
        var tracks = (await audioTrackRepository.GetByIdsAsync(trackIds, cancellationToken))
            .ToDictionary(t => t.Id);

        var layers = mix.Layers
            .Select(layer =>
            {
                var track = tracks[layer.AudioTrackId];
                return new AudioMixLayerResponse(
                    track.Id,
                    track.Name,
                    track.Role,
                    audioFileUrlResolver.BuildUrl(track.FileName),
                    layer.Volume,
                    track.DurationSeconds);
            })
            .ToList();

        return new SessionAudioMixResponse(mix.Id, mix.StressSessionId, mix.CreatedAt, layers);
    }
}