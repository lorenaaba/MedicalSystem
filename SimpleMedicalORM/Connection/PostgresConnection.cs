using Npgsql;
using System.Data;

namespace SimpleMedicalORM.Connection
{
    public class PostgresConnection : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private NpgsqlTransaction? _transaction;

        public PostgresConnection(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public async Task OpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task CloseAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            if (_connection.State != ConnectionState.Closed)
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<NpgsqlTransaction> BeginTransactionAsync()
        {
            await OpenAsync();
            _transaction = await _connection.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params NpgsqlParameter[] parameters)
        {
            await OpenAsync();

            using var command = new NpgsqlCommand(sql, _connection, _transaction);
            command.Parameters.AddRange(parameters);

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string sql, params NpgsqlParameter[] parameters)
        {
            await OpenAsync();

            using var command = new NpgsqlCommand(sql, _connection, _transaction);
            command.Parameters.AddRange(parameters);

            return await command.ExecuteScalarAsync();
        }

        public async Task<NpgsqlDataReader> ExecuteReaderAsync(string sql, params NpgsqlParameter[] parameters)
        {
            await OpenAsync();

            using var command = new NpgsqlCommand(sql, _connection, _transaction);
            command.Parameters.AddRange(parameters);

            return await command.ExecuteReaderAsync();
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<NpgsqlDataReader, T> mapper, params NpgsqlParameter[] parameters)
        {
            var results = new List<T>();

            await using var reader = await ExecuteReaderAsync(sql, parameters);

            while (await reader.ReadAsync())
            {
                results.Add(mapper(reader));
            }

            return results;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}