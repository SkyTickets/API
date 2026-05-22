namespace API.ExportClasses
{
    public class ExportAirport
    {
        public int ApId { get; set; }

        public string ApName { get; set; } = null!;

        public string ApCountry { get; set; } = null!;

        public string ApCity { get; set; } = null!;

        public string ApStreet { get; set; } = null!;

        public string ApBuilding { get; set; } = null!;
    }
}
