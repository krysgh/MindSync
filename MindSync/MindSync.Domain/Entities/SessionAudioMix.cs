using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class SessionAudioMix
{
    private readonly List<SessionAudioMixLayer> _layers = [];

    public Guid Id { get; private set; }
    public Guid StressSessionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<SessionAudioMixLayer> Layers => _layers.AsReadOnly();

    private SessionAudioMix() { }

    public SessionAudioMix(
        Guid id,
        Guid stressSessionId,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O identificador da mixagem (Id) não pode ser vazio.");

        if (stressSessionId == Guid.Empty)
            throw new DomainException("O identificador da sessão de estresse (StressSessionId) não pode ser vazio.");

        Id = id;
        StressSessionId = stressSessionId;
        CreatedAt = createdAt;
    }

    public SessionAudioMix(Guid stressSessionId)
        : this(Guid.NewGuid(), stressSessionId, DateTime.UtcNow)
    {
    }

    public void AddLayer(SessionAudioMixLayer layer) => _layers.Add(layer);

    public SessionAudioMixLayer AddLayer(Guid audioTrackId, decimal volume)
    {
        var layer = new SessionAudioMixLayer(Id, audioTrackId, volume);
        _layers.Add(layer);
        return layer;
    }
}