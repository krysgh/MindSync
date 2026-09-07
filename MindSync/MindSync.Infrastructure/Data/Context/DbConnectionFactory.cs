using Microsoft.Data.SqlClient;
using System.Data;

namespace MindSync.Infrastructure.Data.Context;

public class DbConnectionFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
