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
    [Authorize(Roles = UserRole.ADMIN)]
    public class AdminController : Controller
    {
        BookManager _bookManager;
        BorrowManager _borrowManager;
        AppSettings _appSettings;
        ILogger _logger;

        public AdminController(ILogger<AdminController> logger, IOptionsSnapshot<AppSettings> options, LibDBContext context)
        {
            _bookManager = new BookManager(context);
            _borrowManager = new BorrowManager(context);
            _appSettings = options.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var borrows = await _borrowManager.GetBorrows(isReturned: false);
                var borrowsStat = await _borrowManager.GetStatistics();
                ViewData["BorrowsStatistics"] = borrowsStat;
                ViewData["BooksStatistics"] = await _bookManager.GetBooksCount();
                ViewData["AlertAfter"] = _appSettings.AlertAfter;
                return View(borrows);
            }
            catch (Exception ex)
            {
                return View("_Error", new ErrorVM(ex));
            }
        }

        [HttpGet]
        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(BookCreateVM book)
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage);
                return View(book);
            }
            try
            {
                var created = await _bookManager.CreateBook(book.ISBN, book.Title, book.Author, book.Description, book.Count);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(book);
            }
        }
    }
}
