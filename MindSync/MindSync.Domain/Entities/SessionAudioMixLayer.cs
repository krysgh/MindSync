using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class SessionAudioMixLayer
{
    public Guid Id { get; private set; }
    public Guid SessionAudioMixId { get; private set; }
    public Guid AudioTrackId { get; private set; }
    public decimal Volume { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SessionAudioMixLayer() { }

    public SessionAudioMixLayer(
        Guid id,
        Guid sessionAudioMixId,
        Guid audioTrackId,
        decimal volume,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O identificador da camada de áudio (Id) não pode ser vazio.");

        if (sessionAudioMixId == Guid.Empty)
            throw new DomainException("O identificador da mixagem (SessionAudioMixId) não pode ser vazio.");

        if (audioTrackId == Guid.Empty)
            throw new DomainException("O identificador da faixa de áudio (AudioTrackId) não pode ser vazio.");

        if (volume < 0 || volume > 1)
            throw new DomainException("O volume da camada deve estar entre 0 e 1.");

        Id = id;
        SessionAudioMixId = sessionAudioMixId;
        AudioTrackId = audioTrackId;
        Volume = volume;
        CreatedAt = createdAt;
    }

    public SessionAudioMixLayer(Guid sessionAudioMixId, Guid audioTrackId, decimal volume)
        : this(Guid.NewGuid(), sessionAudioMixId, audioTrackId, volume, DateTime.UtcNow)
    {
    }
}