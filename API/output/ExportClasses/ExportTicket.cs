namespace API.ExportClasses
{
    public class ExportTicket
    {
        public int TId { get; set; }

        public int TBooking { get; set; }

        /// <summary>ФИО пассажира (кириллица)</summary>
        public string TPassenger { get; set; } = null!;

        /// <summary>ФИО пассажира (латиница)</summary>
        public string? TPassengerLatin { get; set; }

        public string TClass { get; set; } = null!;

        public int TPrice { get; set; }

        public List<ExportAdditionalService>? Services { get; set; }

        // Денормализованные поля рейса
        public string? FAirline { get; set; }
        public string? FDepartureAirport { get; set; }
        public string? FArrivalAirport { get; set; }
        public DateTime? FDepartureTime { get; set; }
        public DateTime? FArrivalTime { get; set; }
        public string? AirlineImage { get; set; }
    }
}
