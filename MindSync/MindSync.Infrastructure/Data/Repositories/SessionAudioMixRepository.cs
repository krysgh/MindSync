using Dapper;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Infrastructure.Data.Context;
using System.Data;

namespace MindSync.Infrastructure.Data.Repositories;

public class SessionAudioMixRepository(DbConnectionFactory connectionFactory) : ISessionAudioMixRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<SessionAudioMix?> GetByStressSessionIdAsync(
        Guid stressSessionId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT 
            m.Id, m.StressSessionId, m.CreatedAt,
            l.Id, l.SessionAudioMixId, l.AudioTrackId, l.Volume, l.CreatedAt
        FROM SessionAudioMixes m
        LEFT JOIN SessionAudioMixLayers l ON m.Id = l.SessionAudioMixId
        WHERE m.StressSessionId = @StressSessionId;";

        using var connection = _connectionFactory.CreateConnection();
        var mixLookup = new Dictionary<Guid, SessionAudioMix>();

        await connection.QueryAsync<SessionAudioMix, SessionAudioMixLayer, SessionAudioMix>(
            new CommandDefinition(sql, new { StressSessionId = stressSessionId }, cancellationToken: cancellationToken),
            (mix, layer) =>
            {
                if (!mixLookup.TryGetValue(mix.Id, out var currentMix))
                {
                    currentMix = mix;
                    mixLookup.Add(currentMix.Id, currentMix);
                }

                if (layer != null && layer.Id != Guid.Empty)
                {
                    currentMix.AddLayer(layer);
                }

                return currentMix;
            },
            splitOn: "Id"
        );

        return mixLookup.Values.FirstOrDefault();
    }

    public async Task AddAsync(
        SessionAudioMix mix,
        CancellationToken cancellationToken = default)
    {
        const string insertMixSql = @"
            INSERT INTO SessionAudioMixes (Id, StressSessionId, CreatedAt)
            VALUES (@Id, @StressSessionId, @CreatedAt);";

        const string insertLayerSql = @"
            INSERT INTO SessionAudioMixLayers (Id, SessionAudioMixId, AudioTrackId, Volume, CreatedAt)
            VALUES (@Id, @SessionAudioMixId, @AudioTrackId, @Volume, @CreatedAt);";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                insertMixSql,
                new { mix.Id, mix.StressSessionId, mix.CreatedAt },
                transaction: transaction,
                cancellationToken: cancellationToken));

            foreach (var layer in mix.Layers)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    insertLayerSql,
                    new { layer.Id, layer.SessionAudioMixId, layer.AudioTrackId, layer.Volume, layer.CreatedAt },
                    transaction: transaction,
                    cancellationToken: cancellationToken));
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}