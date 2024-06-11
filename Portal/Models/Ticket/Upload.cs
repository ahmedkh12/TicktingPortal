namespace Portal.Models.Ticket
{
    public class Upload
    {
        public int Id { get; set; }

        public string? UserUploaded { get; set; }

        public int? TicketID { get; set; }

        public DateTime DateUploaded = DateTime.Now;

        public string? ImageURL { get; set; }
    }
}
