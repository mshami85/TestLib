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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        BookManager _bookManager;
        AppSettings _appSettings;
        public HomeController(ILogger<HomeController> logger, LibDBContext libDB, IOptionsSnapshot<AppSettings> options)
        {
            _logger = logger;
            _appSettings = options.Value;
            _bookManager = new BookManager(libDB);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, string order)
        {
            try
            {
                var books = await _bookManager.ListBooksDetails(page ?? 1, _appSettings.PageSize, order ?? "Id");
                var pages = (int)Math.Ceiling(await _bookManager.GetBookCount() / (double)_appSettings.PageSize);
                ViewData["Pages"] = pages;
                return View(books);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                ViewData["Pages"] = 0;
                return View(Enumerable.Empty<BookDetails>());
            }
        }

        [HttpGet]
        public IActionResult Search()
        {
            ViewData["Pages"] = 0;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string? title, string? isbn, string? author, string? description)
        {
            try
            {
                var result = await _bookManager.SearchBooks(title, isbn, author, description);
                return View(result);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View(Enumerable.Empty<BookDetails>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var book = await _bookManager.GetBookDetails(id.Value);
            return View(book);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}