namespace API.ExportClasses
{
public class ExportFlight
{
public int FId { get; set; }

public string FAirline { get; set; } = null!;

public string FDepartureAirport { get; set; } = null!;

public string FArrivalAirport { get; set; } = null!;

public DateTime FDepartureTime { get; set; }

public DateTime FArrivalTime { get; set; }

public int FSeatsCount { get; set; }

public int FAvailableSeats { get; set; }

public int FPrice { get; set;  }

public string? AirlineImage { get; set; }
}
}
