using System;
using System.Collections.Generic;

namespace API.Model1;

public partial class Flight
{
    public int FId { get; set; }

    public int FAirline { get; set; }

    public int FDepartureAirport { get; set; }

    public int FArrivalAirport { get; set; }

    public DateTime FDepartureTime { get; set; }

    public DateTime FArrivalTime { get; set; }

    public int FSeatsCount { get; set; }

    public int FPrice { get; set; }

    public virtual Airline FAirlineNavigation { get; set; } = null!;

    public virtual Airport FArrivalAirportNavigation { get; set; } = null!;

    public virtual Airport FDepartureAirportNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
