using API.ExportClasses;
using System;
using System.Collections.Generic;

namespace API.Model;

public partial class Airline
{
    public int AlId { get; set; }

    public string AlName { get; set; } = null!;

    public string AlEmail { get; set; } = null!;

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public ExportAirline ToExport()
    {
        return new()
        {
            AlId = AlId,
            AlName = AlName,
            AlEmail = AlEmail,
        };
    }
}
