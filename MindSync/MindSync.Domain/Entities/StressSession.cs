using MindSync.Domain.Enums;
using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class StressSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public StressLevel StressLevelBefore { get; private set; }
    public StressLevel? StressLevelAfter { get; private set; }
    public string TargetFrequency { get; private set; } = null!;

    public bool IsCompleted => StressLevelAfter.HasValue;

    private StressSession() { }

    public StressSession(
        Guid id,
        Guid userId,
        DateTime createdAt,
        StressLevel stressLevelBefore,
        string targetFrequency)
    {
        if (userId == Guid.Empty)
            throw new DomainException("O identificador do utilizador (UserId) não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(targetFrequency))
            throw new DomainException("A frequência alvo (TargetFrequency) deve ser preenchida.");

        Id = id;
        UserId = userId;
        CreatedAt = createdAt;
        StressLevelBefore = stressLevelBefore;
        StressLevelAfter = null;
        TargetFrequency = targetFrequency;
    }

    public void CompleteSession(StressLevel stressLevelAfter)
    {
        if (IsCompleted)
            throw new DomainException("Esta sessão já foi finalizada e avaliada.");

        StressLevelAfter = stressLevelAfter;
        EndedAt = DateTime.UtcNow;
    }
}
