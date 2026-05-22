using API.ExportClasses;
using System;
using System.Collections.Generic;

namespace API.Model;

public partial class Airplane
{
    public int PlId { get; set; }

    public string PlModel { get; set; } = null!;

    public int PlEconomySeats { get; set; }

    public int PlComfortSeats { get; set; }

    public int PlBusinessSeats { get; set; }

    public int PlFirstClassSeats { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public ExportAirplane ToExport()
    {
        return new()
        {
            PlId = PlId,
            PlModel = PlModel,
            PlEconomySeats = PlEconomySeats,
            PlComfortSeats = PlComfortSeats,
            PlBusinessSeats = PlBusinessSeats,
            PlFirstClassSeats = PlFirstClassSeats,
        };
    }
}
