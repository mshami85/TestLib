using TestLibrary.Data;

namespace TestLibrary.Models
{
    public class Book : Entity
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public int Count { get; set; }
    }
}
