namespace API.ExportClasses
{
public class ExportTicket
{
public int TId { get; set; }

public int TFlight { get; set; }

public string TUser { get; set; } = null!;

public DateTime TBoughtDate { get; set; }

public string TClass { get; set; } = null!;

public int TTotalPrice { get; set; }

public string TStatus { get; set; } = null!;

public string? TUserLatin { get; set; } = null!;

public string? FAirline { get; set; } = null!;

public string? FDepartureAirport { get; set; } = null!;

public string? FArrivalAirport { get; set; } = null!;

public DateTime? FDepartureTime { get; set; }

public DateTime? FArrivalTime { get; set; }

public string? AirlineImage { get; set; } = null!;

}
}
