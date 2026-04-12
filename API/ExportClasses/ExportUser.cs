namespace API.ExportClasses
{
    public class ExportUser
    {
        public int UId { get; set; }

        public string USurname { get; set; } = null!;

        public string UName { get; set; } = null!;

        public string? UPatronymic { get; set; }

        public string UEmail { get; set; } = null!;

        public string UPassword { get; set; } = null!;

        public string URole { get; set; } = null!;

        public string UPhone { get; set; } = null!;

        public DateOnly UBirthdate { get; set; }

        public decimal UPassportSerial { get; set; }

        public decimal UPassportNumber { get; set; }

        public string? UserImage { get; set; }
    }
}
