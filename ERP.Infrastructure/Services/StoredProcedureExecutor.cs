using System.Data;
using System.Data.Common;
using Dapper;
using ERP.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;

namespace ERP.Infrastructure.Services;

public class StoredProcedureExecutor : IStoredProcedureExecutor
{
    private readonly string _connectionString;

    public StoredProcedureExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<T?> ExecuteSingleAsync<T>(string procedureName, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteNonQueryAsync(string procedureName, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<T> ExecuteScalarAsync<T>(string procedureName, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<DbDataReader> ExecuteReaderAsync(string procedureName, object? parameters = null)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
        
        if (parameters != null)
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var property in properties)
            {
                command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(parameters) ?? DBNull.Value);
            }
        }
        
        return await command.ExecuteReaderAsync();
    }
}