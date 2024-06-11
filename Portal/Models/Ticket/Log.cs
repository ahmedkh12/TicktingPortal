namespace Portal.Models.Ticket
{
    public class Log
    {
        public int Id { get; set; }

        public string? Action { get; set; }

        public DateTime time { get; set; }

        public int? TicketNumber { get; set; }
        public string? Message { get; set; }

        public string? AddedBy { get; set; }

    }
}
