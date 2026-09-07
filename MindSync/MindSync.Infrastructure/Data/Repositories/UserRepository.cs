using Dapper;
using MindSync.Domain.Entities;
using MindSync.Domain.Interfaces;
using MindSync.Domain.ValueObjects;
using MindSync.Infrastructure.Data.Context;

namespace MindSync.Infrastructure.Data.Repositories;

public class UserRepository(DbConnectionFactory connectionFactory) : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;

    public async Task InsertAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, CreatedAt)
            VALUES (@Id, @FirstName, @LastName, @Email, @PasswordHash, @CreatedAt);";

        await connection.ExecuteAsync(sql, new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            Email = user.Email.Value,
            user.PasswordHash,
            user.CreatedAt
        });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, FirstName, LastName, Email, PasswordHash, CreatedAt 
            FROM Users WITH (NOLOCK) 
            WHERE Id = @Id;";

        var result = await connection.QueryAsync<Guid, string, string, string, string, DateTime, User>(
            sql,
            (userId, firstName, lastName, emailStr, passwordHash, createdAt) =>
                new User(userId, firstName, lastName, new Email(emailStr), passwordHash, createdAt),
            new { Id = id },
            splitOn: "FirstName,LastName,Email,PasswordHash,CreatedAt"
        );

        return result.FirstOrDefault();
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, FirstName, LastName, Email, PasswordHash, CreatedAt 
            FROM Users WITH (NOLOCK) 
            WHERE Email = @Email;";

        var result = await connection.QueryAsync<Guid, string, string, string, string, DateTime, User>(
        sql,
        (userId, firstName, lastName, emailStr, passwordHash, createdAt) =>
            new User(userId, firstName, lastName, new Email(emailStr), passwordHash, createdAt),
        new { Email = email.Value },
        splitOn: "FirstName,LastName,Email,PasswordHash,CreatedAt"
    );

        return result.FirstOrDefault();
    }

    public async Task<bool> ExistsByEmailAsync(Email email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT TOP 1 1 FROM Users WITH (NOLOCK) WHERE Email = @Email;";

        var result = await connection.ExecuteScalarAsync<int?>(sql, new { Email = email.Value });
        return result.HasValue;
    }
}
