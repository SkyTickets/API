using API.ExportClasses;
using Microsoft.EntityFrameworkCore;

namespace API.Model;

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

public ExportFlight ToExport()
{
string[] files = [];
if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/")))
{
files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/"));
}

string file = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == FAirline.ToString()) ?? "";

return new()
{
FId = FId,
FAirline = FAirlineNavigation.AlName,
FDepartureAirport = FDepartureAirportNavigation.ApName,
FArrivalAirport = FArrivalAirportNavigation.ApName,
FDepartureTime = FDepartureTime,
FArrivalTime = FArrivalTime,
FSeatsCount = FSeatsCount,
FAvailableSeats = FSeatsCount - Tickets.Count(x => x.TFlight == FId),
FPrice = FPrice,
AirlineImage = "/airline/" + Path.GetFileName(file),
};
}
}
