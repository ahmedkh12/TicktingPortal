
namespace Portal.Models.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First Name is Required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Mobile is Required")]
        [Display(Name = "Mobile")]
        [MaxLength(11)]
        public string Mobile { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [Remote(action: "IsEmailAvailable", controller: "Account")]  // remote validation attribute
        public string Email { get; set; }



        [Required]
        public string Company { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


    }
}
