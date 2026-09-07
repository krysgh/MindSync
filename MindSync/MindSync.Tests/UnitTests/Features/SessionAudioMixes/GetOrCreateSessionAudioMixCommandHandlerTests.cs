using FluentAssertions;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.SessionAudioMixes.Commands.GetOrCreateSessionAudioMix;
using MindSync.Domain.Entities;
using MindSync.Domain.Enums;
using MindSync.Domain.Interfaces;
using NSubstitute;

namespace MindSync.Tests.UnitTests.Features.SessionAudioMixes;

public class GetOrCreateSessionAudioMixCommandHandlerTests
{
    private readonly ISessionAudioMixRepository _mixRepositoryMock = Substitute.For<ISessionAudioMixRepository>();
    private readonly IStressSessionRepository _stressSessionRepositoryMock = Substitute.For<IStressSessionRepository>();
    private readonly IAudioTrackRepository _audioTrackRepositoryMock = Substitute.For<IAudioTrackRepository>();
    private readonly IAudioMixComposer _audioMixComposerMock = Substitute.For<IAudioMixComposer>();
    private readonly IAudioFileUrlResolver _audioFileUrlResolverMock = Substitute.For<IAudioFileUrlResolver>();
    private readonly ICurrentUserService _currentUserServiceMock = Substitute.For<ICurrentUserService>();
    private readonly GetOrCreateSessionAudioMixCommandHandler _handler;

    public GetOrCreateSessionAudioMixCommandHandlerTests()
    {
        _handler = new GetOrCreateSessionAudioMixCommandHandler(
            _mixRepositoryMock,
            _stressSessionRepositoryMock,
            _audioTrackRepositoryMock,
            _audioMixComposerMock,
            _audioFileUrlResolverMock,
            _currentUserServiceMock);
    }

    private static AudioTrack CreateTrack(string role, string? targetFrequency = null) =>
        new(Guid.NewGuid(), $"Faixa {role}", role, targetFrequency, 0, 8, $"{role}.mp3", 600, DateTime.UtcNow);

    [Fact]
    public async Task Handle_ShouldReturnExistingMix_WithoutCallingComposer_WhenThereIsAlreadyOneForTheSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var track = CreateTrack("Binaural", "Theta (6 Hz) - Meditação");
        var existingMix = new SessionAudioMix(sessionId);
        existingMix.AddLayer(track.Id, 1.0m);

        _mixRepositoryMock.GetByStressSessionIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(existingMix);
        _audioTrackRepositoryMock.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                                  .Returns(new List<AudioTrack> { track });
        _audioFileUrlResolverMock.BuildUrl(track.FileName).Returns("http://localhost/audio/tracks/Binaural.mp3");

        // Act
        var result = await _handler.Handle(new GetOrCreateSessionAudioMixCommandRequest(sessionId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(existingMix.Id);
        result.Layers.Should().ContainSingle(l => l.AudioTrackId == track.Id);

        await _stressSessionRepositoryMock.DidNotReceive().GetSessionByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _audioMixComposerMock.DidNotReceive().Compose(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<IEnumerable<AudioTrack>>());
        await _mixRepositoryMock.DidNotReceive().AddAsync(Arg.Any<SessionAudioMix>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoMixExistsForSession_ShouldCreateNewMix()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = new StressSession(sessionId, userId, DateTime.UtcNow, StressLevel.Moderate, "Theta (6 Hz) - Meditação");
        var binauralTrack = CreateTrack("Binaural", "Theta (6 Hz) - Meditação");
        var textureTrack = CreateTrack("Texture");

        _mixRepositoryMock.GetByStressSessionIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns((SessionAudioMix?)null);
        _stressSessionRepositoryMock.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _currentUserServiceMock.UserId.Returns(userId);

        var candidates = new List<AudioTrack> { binauralTrack, textureTrack };
        _audioTrackRepositoryMock.GetCandidatesAsync(session.TargetFrequency, Arg.Any<CancellationToken>()).Returns(candidates);

        _audioMixComposerMock.Compose((int)session.StressLevelBefore, session.TargetFrequency, Arg.Any<IEnumerable<AudioTrack>>())
            .Returns(new List<AudioMixSelection>
            {
                new(binauralTrack.Id, 1.0m),
                new(textureTrack.Id, 0.6m)
            });

        _audioTrackRepositoryMock.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                                  .Returns(candidates);
        _audioFileUrlResolverMock.BuildUrl(Arg.Any<string>()).Returns(callInfo => $"http://localhost/audio/{callInfo.Arg<string>()}");

        // Act
        var result = await _handler.Handle(new GetOrCreateSessionAudioMixCommandRequest(sessionId), CancellationToken.None);

        // Assert
        result.StressSessionId.Should().Be(sessionId);
        result.Layers.Should().HaveCount(2);
        await _mixRepositoryMock.Received(1).AddAsync(Arg.Any<SessionAudioMix>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenSessionBelongsToAnotherUser()
    {
        // Arrange
        var realOwner = Guid.NewGuid();
        var attacker = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = new StressSession(sessionId, realOwner, DateTime.UtcNow, StressLevel.Moderate, "Theta (6 Hz) - Meditação");

        _mixRepositoryMock.GetByStressSessionIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns((SessionAudioMix?)null);
        _stressSessionRepositoryMock.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _currentUserServiceMock.UserId.Returns(attacker);

        // Act
        Func<Task> act = async () => await _handler.Handle(new GetOrCreateSessionAudioMixCommandRequest(sessionId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _mixRepositoryMock.DidNotReceive().AddAsync(Arg.Any<SessionAudioMix>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRereadAndReturnRaceMixing_WhenInsertFailsButAMixAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = new StressSession(sessionId, userId, DateTime.UtcNow, StressLevel.Moderate, "Theta (6 Hz) - Meditação");
        var track = CreateTrack("Binaural", "Theta (6 Hz) - Meditação");

        var raceMix = new SessionAudioMix(sessionId);
        raceMix.AddLayer(track.Id, 1.0m);

        _mixRepositoryMock.GetByStressSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((SessionAudioMix?)null, raceMix);

        _stressSessionRepositoryMock.GetSessionByIdAsync(sessionId, Arg.Any<CancellationToken>()).Returns(session);
        _currentUserServiceMock.UserId.Returns(userId);

        var candidates = new List<AudioTrack> { track };
        _audioTrackRepositoryMock.GetCandidatesAsync(session.TargetFrequency, Arg.Any<CancellationToken>()).Returns(candidates);
        _audioMixComposerMock.Compose(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<IEnumerable<AudioTrack>>())
            .Returns(new List<AudioMixSelection> { new(track.Id, 1.0m) });

        _mixRepositoryMock.When(x => x.AddAsync(Arg.Any<SessionAudioMix>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Violação de índice único simulada"));

        _audioTrackRepositoryMock.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                                  .Returns(candidates);
        _audioFileUrlResolverMock.BuildUrl(Arg.Any<string>()).Returns("http://localhost/audio/track.mp3");

        // Act
        var result = await _handler.Handle(new GetOrCreateSessionAudioMixCommandRequest(sessionId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(raceMix.Id);
    }
}