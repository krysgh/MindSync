using FluentAssertions;
using MindSync.Domain.Entities;
using MindSync.Domain.Exceptions;

namespace MindSync.Tests.UnitTests.Entities;

public class SessionAudioMixTests
{
    [Fact]
    public void Constructor_ShouldCreateEmptyMixage_WithValidData()
    {
        // Act
        var mix = new SessionAudioMix(Guid.NewGuid());

        // Assert
        mix.Id.Should().NotBeEmpty();
        mix.Layers.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenStressSessionIdIdIsEmpty()
    {
        // Act
        Action act = () => new SessionAudioMix(Guid.Empty);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("O identificador da sessão de estresse (StressSessionId) não pode ser vazio.");
    }

    [Fact]
    public void AddLayer_WithTrackIdAndVolume_ShouldAddLinkedLayerMixing()
    {
        // Arrange
        var mix = new SessionAudioMix(Guid.NewGuid());
        var trackId = Guid.NewGuid();

        // Act
        var layer = mix.AddLayer(trackId, 0.8m);

        // Assert
        mix.Layers.Should().ContainSingle();
        layer.SessionAudioMixId.Should().Be(mix.Id);
        layer.AudioTrackId.Should().Be(trackId);
        layer.Volume.Should().Be(0.8m);
    }

    [Fact]
    public void AddLayer_WithMultipleLayers_ShouldKeepAllInCollection()
    {
        // Arrange
        var mix = new SessionAudioMix(Guid.NewGuid());

        // Act
        mix.AddLayer(Guid.NewGuid(), 1.0m);
        mix.AddLayer(Guid.NewGuid(), 0.6m);

        // Assert
        mix.Layers.Should().HaveCount(2);
    }
}