namespace TestLibrary.Helpers
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public int PageSize { get; set; }
        public int AlertAfter { get; set; }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }
}
