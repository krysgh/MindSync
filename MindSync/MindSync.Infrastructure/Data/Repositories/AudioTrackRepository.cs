using Dapper;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Infrastructure.Data.Context;

namespace MindSync.Infrastructure.Data.Repositories;

public class AudioTrackRepository(DbConnectionFactory connectionFactory) : IAudioTrackRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<AudioTrack>> GetCandidatesAsync(
        string targetFrequency,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, Role, TargetFrequency, MinStressLevel, MaxStressLevel, FileName, DurationSeconds, CreatedAt
            FROM AudioTracks
            WHERE (Role = 'Binaural' AND TargetFrequency = @TargetFrequency)
               OR (Role = 'Texture');";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { TargetFrequency = targetFrequency }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<AudioTrack>(command);
    }

    public async Task<IEnumerable<AudioTrack>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, Role, TargetFrequency, MinStressLevel, MaxStressLevel, FileName, DurationSeconds, CreatedAt
            FROM AudioTracks
            WHERE Id IN @Ids;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Ids = ids }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<AudioTrack>(command);
    }
}