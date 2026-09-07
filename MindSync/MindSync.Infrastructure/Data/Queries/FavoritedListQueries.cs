using Dapper;
using MindSync.Application.Features.FavoritedSessionLists.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;
using MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;
using MindSync.Infrastructure.Data.Context;

namespace MindSync.Infrastructure.Data.Queries;

public class FavoritedListQueries(DbConnectionFactory connectionFactory) : IFavoritedListQueries
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IEnumerable<GetFavoritedListsByUserIdQueryResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                fl.Id, 
                fl.Name, 
                fl.CreatedAt, 
                COUNT(fs.Id) AS TotalItems
            FROM FavoritedLists fl
            LEFT JOIN FavoritedSessions fs ON fl.Id = fs.FavoritedListId
            WHERE fl.UserId = @UserId
            GROUP BY fl.Id, fl.Name, fl.CreatedAt
            ORDER BY fl.CreatedAt DESC;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<GetFavoritedListsByUserIdQueryResponse>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<GetFavoritedListByIdQueryResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT 
            fl.Id, fl.UserId, fl.Name, fl.CreatedAt,
            fs.Id, fs.StressSessionId, fs.CustomName, ss.TargetFrequency, ss.StressLevelBefore, ss.StressLevelAfter, fs.FavoritedAt
        FROM FavoritedLists fl
        LEFT JOIN FavoritedSessions fs ON fl.Id = fs.FavoritedListId
        LEFT JOIN StressSessions ss ON fs.StressSessionId = ss.Id
        WHERE fl.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var listLookup = new Dictionary<Guid, GetFavoritedListByIdQueryResponse>();

        await connection.QueryAsync<GetFavoritedListByIdQueryResponse, FavoritedSessionItemResponse, GetFavoritedListByIdQueryResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken),
            (list, item) =>
            {
                if (!listLookup.TryGetValue(list.Id, out var currentList))
                {
                    currentList = list;
                    listLookup.Add(currentList.Id, currentList);
                }

                if (item != null && item.Id != Guid.Empty)
                {
                    currentList.Items.Add(item);
                }

                return currentList;
            },
            splitOn: "Id"
        );

        return listLookup.Values.FirstOrDefault();
    }
}