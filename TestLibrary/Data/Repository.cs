using Microsoft.Data.SqlClient;
using TestLibrary.Data.Ext;
using TestLibrary.Helpers;

namespace TestLibrary.Data
{
    public interface IRepository<T>
    {
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(int page, string order_by = "Id");
    }

    public class Entity
    {
        public int Id { get; set; }
    }

    public class Repository<T> : IRepository<T> where T : Entity, new()
    {
        protected string _tableName;
        protected AppSettings _appSettings;
        protected SqlConnection _con;

        public Repository(AppSettings options, SqlConnection con)
        {
            _tableName = typeof(T).Name + "s";
            _con = con;
            _appSettings = options;
        }

        public async Task<T?> GetAsync(int id)
        {
            string q = $"SELECT * FROM {_tableName} WHERE Id=@id";
            var param = new SqlParameter("@id", id);
            return await _con.QuerySingleAsync<T>(q, param);
        }

        public async Task<IEnumerable<T>> GetAllAsync(int page = 1, string order_by = "Id")
        {
            var page_size = _appSettings.PageSize;
            var q = $"SELECT * FROM {_tableName} ORDER BY {order_by} OFFSET {(page - 1) * page_size} ROWS FETCH NEXT {page_size} ROWS ONLY";
            return await _con.QueryAsync<T>(q);
        }

        public async Task<bool> InsertAsync(T entity)
        {
            var props = typeof(T).GetProperties().Where(p => p.Name != nameof(Entity.Id)).ToArray();
            var q = $"INSERT INTO {_tableName} ({string.Join(", ", props.Select(p => p.Name))}) " +
                    $"VALUES ({string.Join(", ", props.Select(p => "@" + p.Name))})";
            var parameters = props.Select(p => new SqlParameter($"@{p.Name}", p.GetValue(entity) ?? DBNull.Value)).ToArray();

            return await _con.ExecuteAsync<T>(q, parameters);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var props = typeof(T).GetProperties().Where(p => p.Name != nameof(Entity.Id)).ToArray();
            var q = $"UPDATE {_tableName} SET {string.Join(", ", props.Select(p => $"{p.Name}=@{p.Name.ToLower()}"))} WHERE Id=@id";
            var parameters = props.Select(p => new SqlParameter($"@{p.Name.ToLower()}", p.GetValue(entity) ?? DBNull.Value)).ToList();
            parameters.Add(new SqlParameter("@id", entity.Id));
            return await _con.ExecuteAsync<T>(q, parameters.ToArray());
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            string q = $"DELETE FROM {_tableName} WHERE Id=@id";
            var param = new SqlParameter("@id", entity.Id);

            return await _con.ExecuteAsync<T>(q, param);
        }
    }
}
