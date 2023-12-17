using TestLibrary.Data;

namespace TestLibrary.Models
{
    public class Borrow : Entity
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public bool IsReturned { get; set; }
    }
}
