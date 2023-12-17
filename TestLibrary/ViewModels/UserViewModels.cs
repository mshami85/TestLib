using System.ComponentModel.DataAnnotations;

namespace TestLibrary.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "{0} is Required")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool Remember { get; set; }
    }

    public class RegisterationVM
    {
        [Display(Name = "Username")]
        [Required(ErrorMessage = "{0} is Required")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} is Required")]
        public string Password { get; set; }

        [Display(Name = "Full name")]
        [Required(ErrorMessage = "{0} is Required")]
        public string FullName { get; set; }
    }
}
