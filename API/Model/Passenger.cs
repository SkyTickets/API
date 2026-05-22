using API.ExportClasses;
using System;
using System.Collections.Generic;

namespace API.Model;

public partial class Passenger
{
    public int PId { get; set; }

    public string PSurname { get; set; } = null!;

    public string PName { get; set; } = null!;

    public string? PPatronymic { get; set; }

    public DateOnly PBirthdate { get; set; }

    public string PPassportSerial { get; set; } = null!;

    public string PPassportNumber { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ExportPassenger ToExport()
    {
        return new()
        {
            PId = PId,
            PSurname = PSurname,
            PName = PName,
            PPatronymic = PPatronymic,
            PBirthdate = PBirthdate,
            PPassportSerial = PPassportSerial,
            PPassportNumber = PPassportNumber,
        };
    }
}
