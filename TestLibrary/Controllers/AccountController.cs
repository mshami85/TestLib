using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TestLibrary.Data;
using TestLibrary.Helpers;
using TestLibrary.Managers;
using TestLibrary.Models;
using TestLibrary.ViewModels;

namespace TestLibrary.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        UserManager _userManager;
        BorrowManager _borrowManager;
        ILogger<AccountController> _logger;
        AppSettings _appSettings;
        public AccountController(ILogger<AccountController> logger, IOptionsSnapshot<AppSettings> options, LibDBContext context)
        {
            _logger = logger;
            _appSettings = options.Value;
            _userManager = new UserManager(context, User);
            _borrowManager = new BorrowManager(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.GetRole() == UserRole.ADMIN)
            {
                return RedirectToAction("Index", "Admin");
            }
            var borrows = await _borrowManager.GetUserBorrows(User.GetId());
            ViewData["AlertAfter"] = _appSettings.AlertAfter;
            return View(borrows);
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationVM registeration)
        {
            try
            {
                if (await _userManager.Register(registeration))
                {
                    _logger.LogInformation("User {UserName} registered in at {Time}.", registeration.UserName, DateTime.Now);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "Unknown error");
                return View(registeration);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(registeration);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login, string? ReturnUrl = null)
        {
            try
            {
                var userPrincipal = await _userManager.Login(login, ReturnUrl);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = login.Remember,
                    RedirectUri = ReturnUrl ?? "/"
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);
                _logger.LogInformation("User {UserName} logged in at {Time}.", login.UserName, DateTime.Now);

                if(userPrincipal.GetRole()== UserRole.ADMIN)
                {
                    return RedirectToAction("Index", "Admin");
                }
                if (string.IsNullOrEmpty(ReturnUrl))
                {
                    ReturnUrl = "/";
                }
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ReturnUrl = ReturnUrl;
                return View(login);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int? bookId)
        {
            try
            {
                if (bookId == null)
                {
                    ViewData["Error"] = "No book choosed";
                    return View();
                }

                var user_id = User.GetId();
                if (user_id <= 0)
                {
                    ViewData["Error"] = "User not logged in";
                    return View();
                }

                _ = await _borrowManager.Borrow(user_id, bookId.Value);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View("_Error", new ErrorVM(ex));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int? borrowId)
        {
            if (borrowId == null)
            {
                return View("_Error", new ErrorVM("No book choosed"));
            }

            var user_id = User.GetId();
            if (user_id <= 0)
            {
                return View("_Error", new ErrorVM("User not logged in"));
            }

            try
            {
                var borrow = await _borrowManager.GetBorrow(borrowId.Value);
                if (borrow == null || borrow.IsReturned || borrow.UserId != user_id)
                {
                    throw new Exception("Mismatch informations with book borrow");
                }
                var ret = await _borrowManager.Return(borrowId.Value);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View("_Error", new ErrorVM(ex));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
