using TestLibrary.Models;

namespace TestLibrary.ViewModels
{
    public class BorrowDetails : Borrow
    {
        public string User { get; set; }
        public string Book { get; set; }
    }

    public class BorrowStatistics
    {
        public int Total { get; set; }
        public int Returned { get; set; }
    }
}
