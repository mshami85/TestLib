using TestLibrary.Data;

namespace TestLibrary.Models
{
    public class UserRole
    {
        public const string ADMIN = "ADMINISTRATOR";
        public const string USER = "USER";
    }

    public class User : Entity
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
