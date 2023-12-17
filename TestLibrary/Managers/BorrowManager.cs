using Microsoft.Data.SqlClient;
using TestLibrary.Data;
using TestLibrary.Models;
using TestLibrary.ViewModels;

namespace TestLibrary.Managers
{
    public class BorrowManager
    {
        LibDBContext _context;
        public BorrowManager(LibDBContext context)
        {
            _context = context;
        }

        public async Task<Borrow?> GetBorrow(int id)
        {
            return await _context.Borrows.GetAsync(id);
        }

        public async Task<bool> Borrow(int user_id, int book_id)
        {
            var book = await _context.Books.GetAsync(book_id);
            if (book == null)
            {
                throw new Exception("Book not found");
            }
            var user = await _context.Users.GetAsync(user_id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var q = "SELECT * FROM Borrows WHERE BookId=@bid AND UserId=@uid AND IsReturned=0";
            var p1 = new SqlParameter("@bid", book_id);
            var p2 = new SqlParameter("@uid", user_id);
            var same_book = await _context.QueryAsync<Borrow>(q, p1, p2);
            if (same_book.Any())
            {
                throw new Exception("Book already borrowed from user");
            }

            q = "SELECT Books.Count - (SELECT COUNT(*) FROM Borrows WHERE BookId=Books.Id AND IsReturned=0) FROM Books WHERE Id=@bid";
            var param = new SqlParameter("@bid", book_id);
            var available_count = await _context.QueryScalarAsync(q, param);
            if (available_count == null || Convert.ToInt32(available_count) == 0)
            {
                throw new Exception("Cannot borrow book, count is not enough");
            }

            var borrow = new Borrow
            {
                BookId = book_id,
                BorrowDate = DateTime.Now,
                IsReturned = false,
                UserId = user_id,
            };
            return await _context.Borrows.InsertAsync(borrow);
        }

        public async Task<bool> Return(int id)
        {
            var borrow = await _context.Borrows.GetAsync(id);
            if (borrow == null)
            {
                throw new Exception("Borrow not found");
            }
            borrow.IsReturned = true;
            return await _context.Borrows.UpdateAsync(borrow);
        }

        public async Task<IEnumerable<BorrowDetails>> GetUserBorrows(int user_id)
        {
            var user = await _context.Users.GetAsync(user_id);
            if (user == null)
            {
                throw new NullReferenceException("User not found");
            }
            var q = $"SELECT Borrows.*,Books.Title AS Book,Users.FullName AS \"User\" FROM Borrows " +
                    $"INNER JOIN Books ON Books.Id=BookId " +
                    $"INNER JOIN Users ON Users.Id=UserId WHERE UserId=@id";
            SqlParameter parameter = new("@id", user_id);
            return await _context.QueryAsync<BorrowDetails>(q, parameter);
        }

        public async Task<IEnumerable<BorrowDetails>> GetBorrows(bool? isReturned = null)
        {
            var q = $"SELECT Borrows.*,Books.Title AS Book,Users.FullName AS \"User\" FROM Borrows " +
                    $"INNER JOIN Books ON Books.Id=BookId " +
                    $"INNER JOIN Users ON Users.Id=UserId ";
            if (isReturned.HasValue)
            {
                q += "WHERE IsReturned=" + (isReturned.Value ? "1" : "0");
            }
            return await _context.QueryAsync<BorrowDetails>(q);
        }

        public async Task<BorrowStatistics?> GetStatistics()
        {
            var q = "SELECT COUNT(*) AS Total,(SELECT COUNT(*) FROM Borrows WHERE IsReturned=1) AS Returned FROM Borrows";
            return await _context.QuerySingleAsync<BorrowStatistics>(q);
        }
    }
}
