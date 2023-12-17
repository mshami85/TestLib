using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using TestLibrary.Data;
using TestLibrary.Helpers;
using TestLibrary.Models;
using TestLibrary.ViewModels;

namespace TestLibrary.Managers
{
    public class UserManager
    {
        private readonly LibDBContext _context;
        ClaimsPrincipal _user;

        public UserManager(LibDBContext context, ClaimsPrincipal user)
        {
            _context = context;
            _user = user;
        }

        async Task<User?> CheckLogin(string username, string password)
        {
            var q = $"SELECT * FROM Users WHERE UserName=@username AND Password=@password";
            var p1 = new SqlParameter("@username", username);
            var p2 = new SqlParameter("@password", password);

            return await _context.QuerySingleAsync<User>(q, p1, p2);
        }

        public async Task<ClaimsPrincipal> Login(LoginVM login, string? returnUrl)
        {
            if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
            {
                throw new ArgumentException("Either username or password is empty");
            }

            var user = await CheckLogin(login.UserName, login.Password.GetHash());
            if (user == null)
            {
                throw new ArgumentException("username or password error"); ;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Surname, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);
            return principal;
        }

        public async Task<bool> Register(RegisterationVM registeration)
        {
            if (string.IsNullOrEmpty(registeration.FullName) || string.IsNullOrEmpty(registeration.UserName) || string.IsNullOrEmpty(registeration.Password))
            {
                throw new ArgumentNullException("Missing registeration information");
            }
            var exists = await _context.QueryAsync<User>("SELECT * FROM Users WHERE UserName Like @uname", new SqlParameter("@uname", registeration.UserName));
            if (exists.Any())
            {
                throw new Exception("User name already exists");
            }
            var usr = new User
            {
                UserName = registeration.UserName,
                Password = registeration.Password.GetHash(),
                FullName = registeration.FullName,
                Role = UserRole.USER
            };
            return await _context.Users.InsertAsync(usr);
        }

    }
}
