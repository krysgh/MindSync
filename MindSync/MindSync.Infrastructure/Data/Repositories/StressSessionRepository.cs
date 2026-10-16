using Dapper;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Infrastructure.Data.Context;

namespace MindSync.Infrastructure.Data.Repositories;

public class StressSessionRepository(DbConnectionFactory connectionFactory) : IStressSessionRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task AddAsync(
        StressSession session,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO StressSessions (Id, UserId, CreatedAt, EndedAt, StressLevelBefore, StressLevelAfter, TargetFrequency)
            VALUES (@Id, @UserId, @CreatedAt, @EndedAt, @StressLevelBefore, @StressLevelAfter, @TargetFrequency);";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, session, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<StressSession>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, UserId, CreatedAt, EndedAt, StressLevelBefore, StressLevelAfter, TargetFrequency 
            FROM StressSessions WITH (NOLOCK) 
            WHERE UserId = @UserId 
            ORDER BY CreatedAt DESC;";

        return await connection.QueryAsync<StressSession>(sql, new { UserId = userId });
    }

    public async Task UpdateAsync(
        StressSession session,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE StressSessions
            SET EndedAt = @EndedAt,
                StressLevelAfter = @StressLevelAfter
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, session, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<StressSession?> GetSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, CreatedAt, EndedAt, StressLevelBefore, StressLevelAfter, TargetFrequency
            FROM StressSessions
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = sessionId }, cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<StressSession>(command);
    }
}