using System;
using System.Collections.Generic;

namespace API.Model1;

public partial class Airline
{
    public int AlId { get; set; }

    public string AlName { get; set; } = null!;

    public string AlEmail { get; set; } = null!;

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
