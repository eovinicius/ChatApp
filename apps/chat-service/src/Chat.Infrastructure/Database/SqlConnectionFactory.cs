using System.Data;
using Npgsql;
using Chat.Application.Abstractions.Data;
using Microsoft.Extensions.Configuration;

namespace Chat.Infrastructure.Database;

internal class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database") ?? throw new ArgumentNullException("Connection string 'Database' not found");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
