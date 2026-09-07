using MediatR;

namespace MindSync.Application.Features.SessionAudioMixes.Commands.GetOrCreateSessionAudioMix;

public record GetOrCreateSessionAudioMixCommandRequest(Guid StressSessionId) : IRequest<SessionAudioMixResponse>;

public record SessionAudioMixResponse(
    Guid Id,
    Guid StressSessionId,
    DateTime CreatedAt,
    List<AudioMixLayerResponse> Layers);

public record AudioMixLayerResponse(
    Guid AudioTrackId,
    string Name,
    string Role,
    string AudioUrl,
    decimal Volume,
    int DurationSeconds);