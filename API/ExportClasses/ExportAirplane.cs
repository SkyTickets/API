namespace API.ExportClasses
{
    public class ExportAirplane
    {
        public int PlId { get; set; }

        public string PlModel { get; set; } = null!;

        public int PlEconomySeats { get; set; }

        public int PlComfortSeats { get; set; }

        public int PlBusinessSeats { get; set; }

        public int PlFirstClassSeats { get; set; }
    }
}
