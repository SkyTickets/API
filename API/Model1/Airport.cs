using System;
using System.Collections.Generic;

namespace API.Model1;

public partial class Airport
{
    public int ApId { get; set; }

    public string ApName { get; set; } = null!;

    public string ApCountry { get; set; } = null!;

    public string ApCity { get; set; } = null!;

    public string ApStreet { get; set; } = null!;

    public string ApBuilding { get; set; } = null!;

    public virtual ICollection<Flight> FlightFArrivalAirportNavigations { get; set; } = new List<Flight>();

    public virtual ICollection<Flight> FlightFDepartureAirportNavigations { get; set; } = new List<Flight>();
}
