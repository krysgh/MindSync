using MindSync.Domain.Exceptions;

namespace MindSync.Domain.Entities;

public class AudioTrack
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public string? TargetFrequency { get; private set; }
    public int MinStressLevel { get; private set; }
    public int MaxStressLevel { get; private set; }
    public string FileName { get; private set; } = null!;
    public int DurationSeconds { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AudioTrack() { }

    public AudioTrack(
        Guid id,
        string name,
        string role,
        string? targetFrequency,
        int minStressLevel,
        int maxStressLevel,
        string fileName,
        int durationSeconds,
        DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("O identificador da faixa de áudio (Id) não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da faixa de áudio não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("O papel (Role) da faixa de áudio não pode ser vazio.");

        if (minStressLevel > maxStressLevel)
            throw new DomainException("O nível mínimo de estresse não pode ser maior que o máximo.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("O nome do arquivo da faixa de áudio não pode ser vazio.");

        if (durationSeconds <= 0)
            throw new DomainException("A duração da faixa de áudio deve ser maior que zero.");

        Id = id;
        Name = name;
        Role = role;
        TargetFrequency = targetFrequency;
        MinStressLevel = minStressLevel;
        MaxStressLevel = maxStressLevel;
        FileName = fileName;
        DurationSeconds = durationSeconds;
        CreatedAt = createdAt;
    }

    public bool MatchesStressLevel(int stressLevel) =>
        stressLevel >= MinStressLevel && stressLevel <= MaxStressLevel;
}