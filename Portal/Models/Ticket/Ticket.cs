namespace Portal.Models.Ticket
{
    public class Ticket
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Ticket Title")]
        public string? Title { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string? Description { get; set; }


        [Required]
        [Display(Name = "Customer Email")]
        public string? AuthorEmail { get; set; }
        [Required, MaxLength(11)]
        [Display(Name = "Customer Mobile")]
        public string? AuthorMobile { get; set; }

        [Required]
        [Display(Name = "Company")]
        public string? AuthorCompany { get; set; }

        [Display(Name = "Time Created")]
        public DateTime Created { get; set; }

        [Required]
        public string? status { get; set; }


        public string? useradded { get; set; }
    }
}
