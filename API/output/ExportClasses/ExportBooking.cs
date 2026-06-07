namespace API.ExportClasses
{
    public class ExportBooking
    {
        public int BId { get; set; }

        public int BUser { get; set; }

        public int BFlight { get; set; }

        public DateTime BCreatedAt { get; set; }

        public string BStatus { get; set; } = null!;

        public int BTotalPrice { get; set; }

        public string? BUserName { get; set; }

        public string? FAirline { get; set; }

        public string? FDepartureAirport { get; set; }

        public string? FArrivalAirport { get; set; }

        public DateTime? FDepartureTime { get; set; }

        public DateTime? FArrivalTime { get; set; }

        public List<ExportTicket>? Tickets { get; set; }
    }
}
