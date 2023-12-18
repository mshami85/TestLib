using System.ComponentModel.DataAnnotations;
using TestLibrary.Models;

namespace TestLibrary.ViewModels
{
    public class BookDetails : Book
    {
        public int Available { get; set; }
    }

    public class BookCreateVM
    {
        [Display(Name = "Copy count")]
        [Required(ErrorMessage = "{0} is required"), Range(1, 100)]
        public int Count { get; set; } = 1;

        [Display(Name = "Book title")]
        [Required(ErrorMessage = "{0} is required")]
        public string Title { get; set; }

        [Display(Name = "Book description")]
        public string? Description { get; set; }

        [Display(Name = "Book Author's name")]
        public string? Author { get; set; }

        [Display(Name = "Book ISBN")]
        [Required(ErrorMessage = "{0} is required")]
        public string ISBN { get; set; }
    }
}
