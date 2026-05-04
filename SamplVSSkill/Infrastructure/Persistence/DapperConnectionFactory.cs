using System.Data;
using Dapper;
using Npgsql;
using SamplVSSkill.Domain.Enums;

namespace SamplVSSkill.Infrastructure.Persistence;

/// <summary>
/// Factory for creating raw IDbConnection instances used by Dapper for read (SELECT) queries.
/// Registered as Singleton in DI — only holds the connection string, not an open connection.
/// Registers Dapper TypeHandlers for domain enums on construction.
/// </summary>
public sealed class DapperConnectionFactory
{
    private readonly string _connectionString;

    public DapperConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;

        // Register TypeHandler so Dapper maps string columns → MedicalCenterType enum
        SqlMapper.AddTypeHandler(new EnumTypeHandler<MedicalCenterType>());
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}

/// <summary>
/// Generic Dapper TypeHandler that parses a string DB value into any enum T.
/// </summary>
file sealed class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override T Parse(object value) =>
        Enum.TryParse<T>(value?.ToString(), ignoreCase: true, out var result)
            ? result
            : default;

    public override void SetValue(IDbDataParameter parameter, T value) =>
        parameter.Value = value.ToString();
}

