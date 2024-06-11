// custome user class attribute 
namespace Portal.Models.Account
{

    //this is the custom user attributes
    public class ApplicationUser : IdentityUser

    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]

        public string? Mobile { get; set; }
        [Required]
        public string? Company { get; set; }


    }
}
