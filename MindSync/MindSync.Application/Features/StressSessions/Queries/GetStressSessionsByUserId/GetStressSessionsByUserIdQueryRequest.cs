using MediatR;

namespace MindSync.Application.Features.StressSessions.Queries.GetStressSessionsByUserId;

public record GetStressSessionsByUserIdQueryRequest(Guid UserId) : IRequest<IEnumerable<GetStressSessionsByUserIdQueryResponse>>;