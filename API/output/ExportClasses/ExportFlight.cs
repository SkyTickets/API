namespace API.ExportClasses
{
    public class ExportFlight
    {
        public int FId { get; set; }

        public string FAirline { get; set; } = null!;

        public string? FAirplane { get; set; }

        public string FDepartureAirport { get; set; } = null!;

        public string FArrivalAirport { get; set; } = null!;

        public DateTime FDepartureTime { get; set; }

        public DateTime FArrivalTime { get; set; }

        public int FBasePrice { get; set; }

        public int FEconomySeats { get; set; }
        
        public int FComfortSeats { get; set; }
        
        public int FBusinessSeats { get; set; }
        
        public int FFirstClassSeats { get; set; }

        public int FAvailableEconomySeats { get; set; }

        public int FAvailableComfortSeats { get; set; }

        public int FAvailableBusinessSeats { get; set; }

        public int FAvailableFirstClassSeats { get; set; }

        public string? AirlineImage { get; set; }
    }
}
