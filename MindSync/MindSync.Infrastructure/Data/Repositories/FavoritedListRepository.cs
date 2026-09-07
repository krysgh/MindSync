using Dapper;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Infrastructure.Data.Context;

namespace MindSync.Infrastructure.Data.Repositories;

public class FavoritedListRepository(DbConnectionFactory connectionFactory) : IFavoritedListRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task AddAsync(
        FavoritedList list,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO FavoritedLists (Id, UserId, Name, CreatedAt)
            VALUES (@Id, @UserId, @Name, @CreatedAt);";

        var command = new CommandDefinition(sql, new
        {
            list.Id,
            list.UserId,
            list.Name,
            list.CreatedAt
        }, cancellationToken: cancellationToken);

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(command);
    }

    public async Task<FavoritedList?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT 
            fl.Id, fl.UserId, fl.Name, fl.CreatedAt,
            fs.Id, fs.FavoritedListId, fs.StressSessionId, fs.CustomName, fs.FavoritedAt
        FROM FavoritedLists fl
        LEFT JOIN FavoritedSessions fs ON fl.Id = fs.FavoritedListId
        WHERE fl.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var listLookup = new Dictionary<Guid, FavoritedList>();

        await connection.QueryAsync<FavoritedList, FavoritedSession, FavoritedList>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken),
            (list, session) =>
            {
                if (!listLookup.TryGetValue(list.Id, out var currentList))
                {
                    currentList = list;
                    listLookup.Add(currentList.Id, currentList);
                }

                if (session != null && session.Id != Guid.Empty)
                {
                    currentList.AddItem(session);
                }

                return currentList;
            },
            splitOn: "Id"
        );

        return listLookup.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<FavoritedList>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, Name, CreatedAt 
            FROM FavoritedLists 
            WHERE UserId = @UserId 
            ORDER BY CreatedAt DESC;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);
        return await connection.QueryAsync<FavoritedList>(command);
    }

    public async Task UpdateAsync(
        FavoritedList list,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE FavoritedLists 
            SET Name = @Name 
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new
        {
            list.Id,
            list.Name
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM FavoritedLists WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task AddSessionAsync(
        FavoritedSession favoritedSession,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO FavoritedSessions (Id, FavoritedListId, StressSessionId, CustomName, FavoritedAt)
            VALUES (@Id, @FavoritedListId, @StressSessionId, @CustomName, @FavoritedAt);";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new
        {
            favoritedSession.Id,
            favoritedSession.FavoritedListId,
            favoritedSession.StressSessionId,
            favoritedSession.CustomName,
            favoritedSession.FavoritedAt
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task UpdateSessionAsync(
        FavoritedSession favoritedSession,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE FavoritedSessions 
            SET CustomName = @CustomName 
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new
        {
            favoritedSession.Id,
            favoritedSession.CustomName
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task RemoveSessionAsync(
        Guid favoritedSessionId,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM FavoritedSessions WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = favoritedSessionId }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<FavoritedSession?> GetSessionByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT Id, FavoritedListId, StressSessionId, CustomName, FavoritedAt 
        FROM FavoritedSessions 
        WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<FavoritedSession>(command);
    }

    public async Task<bool> ExistsByNameAndUserIdAsync(
        string name,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1 
            FROM FavoritedLists 
            WHERE UserId = @UserId AND LOWER(Name) = LOWER(@Name)
        ) THEN 1 ELSE 0 END;";

        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserId = userId, Name = name }, cancellationToken: cancellationToken)
        );
    }

    public async Task<bool> ExistsByCustomNameAndListIdAsync(
        string customName,
        Guid favoritedListId,
        Guid? excludingSessionId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM FavoritedSessions
            WHERE FavoritedListId = @FavoritedListId
              AND LOWER(CustomName) = LOWER(@CustomName)
              AND (@ExcludingSessionId IS NULL OR Id <> @ExcludingSessionId)
        ) THEN 1 ELSE 0 END;";

        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { FavoritedListId = favoritedListId, CustomName = customName, ExcludingSessionId = excludingSessionId },
                cancellationToken: cancellationToken)
        );
    }
}