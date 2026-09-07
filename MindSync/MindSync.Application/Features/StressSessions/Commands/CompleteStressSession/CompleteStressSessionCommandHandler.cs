using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.StressSessions.Commands.CompleteStressSession;

public class CompleteStressSessionCommandHandler(
    IStressSessionRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<CompleteStressSessionCommandRequest, Unit>
{
    public async Task<Unit> Handle(
        CompleteStressSessionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var session = await repository.GetSessionByIdAsync(request.SessionId, cancellationToken);

        if (session is null || session.UserId != currentUserService.UserId)
            throw new NotFoundException("Sessão de estresse não encontrada.");

        session.CompleteSession(request.StressLevelAfter);

        await repository.UpdateAsync(session, cancellationToken);

        return Unit.Value;
    }
}