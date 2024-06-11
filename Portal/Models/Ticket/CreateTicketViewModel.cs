namespace Portal.Models.Ticket
{
    public class CreateTicketViewModel
    {

        [Required]
        [Display(Name = "Ticket Title")]
        public string? Title { get; set; }
        [Required]
        [Display(Name = "Ticket Description")]
        public string? Description { get; set; }
        [Required]
        [Display(Name = "Contact Email")]
        public string? AuthorEmail { get; set; }

        [Required, MaxLength(11)]

        [Display(Name = "Contact Mobile")]
        public string? AuthorMobile { get; set; }
        [Required]
        [Display(Name = "Company Name")]
        public string? AuthorCompany { get; set; }



    }
}
