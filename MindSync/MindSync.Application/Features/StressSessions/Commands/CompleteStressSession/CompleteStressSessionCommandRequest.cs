using MediatR;
using MindSync.Domain.Enums;

namespace MindSync.Application.Features.StressSessions.Commands.CompleteStressSession;

public record CompleteStressSessionCommandRequest(
    Guid SessionId,
    StressLevel StressLevelAfter) : IRequest<Unit>;