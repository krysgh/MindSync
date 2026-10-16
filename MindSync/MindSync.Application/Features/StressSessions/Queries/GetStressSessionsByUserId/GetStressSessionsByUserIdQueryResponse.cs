namespace MindSync.Application.Features.StressSessions.Queries.GetStressSessionsByUserId;

public record GetStressSessionsByUserIdQueryResponse(
    Guid Id,
    DateTime CreatedAt,
    DateTime? EndedAt,
    int StressLevelBefore,
    int? StressLevelAfter,
    string TargetFrequency);