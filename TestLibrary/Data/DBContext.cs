using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TestLibrary.Data.Ext;
using TestLibrary.Helpers;
using TestLibrary.Models;

namespace TestLibrary.Data
{
    public interface IDbActions
    {
        Task<IEnumerable<T>> QueryAsync<T>(string query, params SqlParameter[] parameters) where T : class, new();
        Task<T?> QuerySingleAsync<T>(string query, params SqlParameter[] parameters) where T : class, new();
        Task<object> QueryScalarAsync(string query, params SqlParameter[] parameters);
        Task<bool> ExecuteAsync<T>(string query, params SqlParameter[] parameters) where T : class, new();
    }

    public class LibDBContext : IDisposable, IDbActions
    {
        public IRepository<User> Users => _userRepository ??= new GenericRepository<User>(_options, _connection);
        public IRepository<Book> Books => _bookRepository ??= new GenericRepository<Book>(_options, _connection);
        public IRepository<Borrow> Borrows => _borrowRepository ??= new GenericRepository<Borrow>(_options, _connection);

        private readonly SqlConnection _connection;
        private readonly AppSettings _options;

        private IRepository<User>? _userRepository;
        private IRepository<Book>? _bookRepository;
        private IRepository<Borrow>? _borrowRepository;


        public LibDBContext(IOptionsSnapshot<AppSettings> options)
        {
            _options = options.Value;
            _connection = new SqlConnection(_options.ConnectionStrings.DefaultConnection);
            OpenConnection();
        }

        private async void OpenConnection()
        {
            await _connection.OpenAsync();
        }

        #region Dispose
        bool disposed = false;
        public void Dispose()
        {
            Dispose(!disposed);
            disposed = true;
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
        }
        #endregion

        public Task<IEnumerable<T>> QueryAsync<T>(string query, params SqlParameter[] parameters) where T : class, new()
        {
            return _connection.QueryAsync<T>(query, parameters);
        }

        public Task<T?> QuerySingleAsync<T>(string query, params SqlParameter[] parameters) where T : class, new()
        {
            return _connection.QuerySingleAsync<T>(query, parameters);
        }

        public Task<bool> ExecuteAsync<T>(string query, params SqlParameter[] parameters) where T : class, new()
        {
            return _connection.ExecuteAsync<T>(query, parameters);
        }

        public Task<object> QueryScalarAsync(string query, params SqlParameter[] parameters)
        {
            return _connection.QueryScalarAsync(query, parameters);
        }
    }
}
