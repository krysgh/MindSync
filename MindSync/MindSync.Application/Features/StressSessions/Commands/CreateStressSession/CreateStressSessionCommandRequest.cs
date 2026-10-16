using MediatR;
using MindSync.Domain.Enums;

namespace MindSync.Application.Features.StressSessions.Commands.CreateStressSession;

public record CreateStressSessionCommandRequest(
    StressLevel StressLevelBefore,
    string TargetFrequency) : IRequest<Guid>;