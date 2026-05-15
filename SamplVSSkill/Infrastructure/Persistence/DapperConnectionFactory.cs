using System.Data;
using Dapper;
using Npgsql;

namespace SamplVSSkill.Infrastructure.Persistence;

/// <summary>
/// Factory for creating raw IDbConnection instances used by Dapper for read (SELECT) queries.
/// Registered as Singleton in DI — only holds the connection string, not an open connection.
/// </summary>
public sealed class DapperConnectionFactory
{
    private readonly string _connectionString;

    public DapperConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
