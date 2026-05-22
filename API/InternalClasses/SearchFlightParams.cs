namespace API.InternalClasses
{
    public class SearchFlightParams
    {
        public string CityFrom { get; set; } = null!;

        public string CityTo { get; set; } = null!;

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int MinCost { get; set; } = -1;

        public int MaxCost { get; set; } = -1;

        public string? Airline { get; set; }
    }
}
