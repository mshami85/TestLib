namespace TestLibrary.ViewModels
{
    public class ErrorVM
    {
        public ErrorVM(string message)
        {
            Message = message;
        }
        public ErrorVM(Exception ex) : this(ex.Message)
        {

        }
        public string Message { get; }
    }
}
