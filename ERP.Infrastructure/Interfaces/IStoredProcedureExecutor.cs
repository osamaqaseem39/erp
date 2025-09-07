using System.Data;
using System.Data.Common;

namespace ERP.Infrastructure.Interfaces;

public interface IStoredProcedureExecutor
{
    Task<T?> ExecuteSingleAsync<T>(string procedureName, object? parameters = null);
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string procedureName, object? parameters = null);
    Task<int> ExecuteNonQueryAsync(string procedureName, object? parameters = null);
    Task<T> ExecuteScalarAsync<T>(string procedureName, object? parameters = null);
    Task<DbDataReader> ExecuteReaderAsync(string procedureName, object? parameters = null);
}