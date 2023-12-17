using Microsoft.Data.SqlClient;

namespace TestLibrary.Data.Ext
{
    public static class ConnectionExt
    {
        public static async Task<IEnumerable<T>> QueryAsync<T>(this SqlConnection con, string query, params SqlParameter[] parameters) where T : class, new()
        {
            using var command = con.CreateCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            using var reader = await command.ExecuteReaderAsync();
            var entities = new List<T>();
            while (await reader.ReadAsync())
            {
                var entity = reader.Map<T>();
                entities.Add(entity);
            }
            return entities;
        }

        public static async Task<T?> QuerySingleAsync<T>(this SqlConnection con, string query, params SqlParameter[] parameters) where T : class, new()
        {
            using var command = con.CreateCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.Map<T>();
            }
            return default;
        }

        public static async Task<bool> ExecuteAsync<T>(this SqlConnection con, string query, params SqlParameter[] parameters)
        {
            using var command = con.CreateCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            return await command.ExecuteNonQueryAsync() > 0;
        }

        public static async Task<object> QueryScalarAsync(this SqlConnection con, string query, params SqlParameter[] parameters)
        {
            using var command = con.CreateCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
            return await command.ExecuteScalarAsync();
        }
    }
}
