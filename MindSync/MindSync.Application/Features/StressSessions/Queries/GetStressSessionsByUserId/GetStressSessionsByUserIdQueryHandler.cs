using MediatR;
using MindSync.Application.Common.Exceptions;
using MindSync.Application.Common.Interfaces;
using MindSync.Domain.Interfaces;

namespace MindSync.Application.Features.StressSessions.Queries.GetStressSessionsByUserId;

public class GetStressSessionsByUserIdQueryHandler(
    IStressSessionRepository repository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetStressSessionsByUserIdQueryRequest, IEnumerable<GetStressSessionsByUserIdQueryResponse>>
{
    public async Task<IEnumerable<GetStressSessionsByUserIdQueryResponse>> Handle(
        GetStressSessionsByUserIdQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId != currentUserService.UserId)
            throw new NotFoundException("Nenhuma sessão de estresse foi encontrada para este usuário.");

        var sessions = await repository.GetByUserIdAsync(request.UserId);

        return sessions.Select(session => new GetStressSessionsByUserIdQueryResponse(
            session.Id,
            session.CreatedAt,
            session.EndedAt,
            (int)session.StressLevelBefore,
            session.StressLevelAfter.HasValue ? (int)session.StressLevelAfter.Value : null,
            session.TargetFrequency));
    }
}