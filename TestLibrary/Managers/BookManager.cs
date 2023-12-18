using Microsoft.Data.SqlClient;
using TestLibrary.Data;
using TestLibrary.Models;
using TestLibrary.ViewModels;

namespace TestLibrary.Managers
{
    public class BookManager
    {
        private readonly LibDBContext _context;
        public BookManager(LibDBContext context)
        {
            _context = context;
        }

        public async Task<int> GetBooksCount()
        {
            var q = "SELECT COUNT(*) FROM Books";
            return (int)await _context.QueryScalarAsync(q);
        }

        public async Task<IEnumerable<BookDetails>> ListBooksDetails(int page, int size, string order_by)
        {
            var q = "SELECT books.*, " +
                    "Books.Count - (SELECT COUNT(*) FROM Borrows WHERE BookId=Books.Id AND IsReturned=0) AS Available FROM Books " +
                    $"ORDER BY {order_by} OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY";
            var result = await _context.QueryAsync<BookDetails>(q);
            return result;
        }

        //public async Task<IEnumerable<Borrow>> GetBookBorrows(int id)
        //{
        //    var q = "SELECT * FROM Borrows WHERE BookId=@bid";
        //    return await _context.QueryAsync<Borrow>(q, new SqlParameter("@bid", id));
        //}

        public async Task<Book?> GetBook(int id)
        {
            return await _context.Books.GetAsync(id);
        }
        public async Task<BookDetails?> GetBookDetails(int id)
        {
            var q = "SELECT books.*, Books.Count - (SELECT COUNT(*) FROM Borrows WHERE BookId=Books.Id AND IsReturned=0) AS Available FROM Books WHERE Id=@id";
            return await _context.QuerySingleAsync<BookDetails>(q, new SqlParameter("@id", id));
        }

        public async Task<IEnumerable<Book>> SearchBooks(string? title, string? isbn, string? author, string? description)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(isbn) && string.IsNullOrEmpty(author) && string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("Search parameters null");
            }
            var q = "SELECT * FROM Books";

            Dictionary<string, object> parameters = new();
            List<string> where = new();

            if (!string.IsNullOrEmpty(isbn))
            {
                where.Add($" ISBN LIKE @isbn ");
                parameters.Add("@isbn", isbn);
            }
            else
            {
                List<string> like = new();
                if (!string.IsNullOrEmpty(title))
                {
                    like.Clear();
                    var parts = title.Split(' ', ',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        like.Add($"Title LIKE @title{i}");
                        parameters.Add($"@title{i}", "%" + parts[i] + "%");
                    }
                    where.Add(string.Join(" OR ", like));
                }
                if (!string.IsNullOrEmpty(author))
                {
                    like.Clear();
                    var parts = author.Split(' ', ',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        like.Add($"Author LIKE @author{i}");
                        parameters.Add($"@author{i}", "%" + parts[i] + "%");
                    }
                    where.Add(string.Join(" OR ", like));
                }
                if (!string.IsNullOrEmpty(description))
                {
                    like.Clear();
                    var parts = description.Split(' ', ',');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        like.Add($"Description LIKE @descr{i}");
                        parameters.Add($"@descr{i}", "%" + parts[i] + "%");
                    }
                    where.Add(string.Join(" OR ", like));
                }
            }

            if (where.Any())
            {
                q += " WHERE (" + string.Join(") AND (", where) + ")";
            }
            var sqlParams = parameters.Select(p => new SqlParameter(p.Key, p.Value)).ToArray();
            return await _context.QueryAsync<Book>(q, sqlParams);
        }

        internal async Task<bool> CreateBook(string iSBN, string title, string? author, string? description, int count)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(iSBN) || count <= 0)
            {
                throw new ArgumentNullException("Required arguments are not provided");
            }

            var q = "SELECT COUNT(*) FROM Books WHERE ISBN LIKE @isbn OR (Title LIKE @title AND Author LIKE @author)";
            var p1 = new SqlParameter("@isbn", iSBN);
            var p2 = new SqlParameter("@title", title);
            object authValue = string.IsNullOrEmpty(author) ? DBNull.Value : author;
            var p3 = new SqlParameter("@author", authValue);
            var exists = await _context.QueryScalarAsync(q, p1, p2, p3);
            if (exists != null && Convert.ToInt32(exists) > 0)
            {
                throw new Exception("Book already exists");
            }

            var book = new Book
            {
                Author = author,
                Count = count,
                Description = description,
                ISBN = iSBN,
                Title = title
            };
            return await _context.Books.InsertAsync(book);
        }
    }
}
