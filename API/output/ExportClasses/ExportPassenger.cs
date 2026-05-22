namespace API.ExportClasses
{
    public class ExportPassenger
    {
        public int PId { get; set; }

        public string PSurname { get; set; } = null!;

        public string PName { get; set; } = null!;

        public string? PPatronymic { get; set; }

        public DateOnly PBirthdate { get; set; }

        public string PPassportSerial { get; set; } = null!;

        public string PPassportNumber { get; set; } = null!;
    }
}
