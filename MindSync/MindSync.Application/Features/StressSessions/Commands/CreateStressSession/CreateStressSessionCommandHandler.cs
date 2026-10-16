using MediatR;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.StressSessions.Commands.CreateStressSession;

public class CreateStressSessionCommandHandler(
    IStressSessionRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<CreateStressSessionCommandRequest, Guid>
{
    public async Task<Guid> Handle(
        CreateStressSessionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var session = new StressSession(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            request.StressLevelBefore,
            request.TargetFrequency
        );

        await repository.AddAsync(session, cancellationToken);

        return session.Id;
    }
}