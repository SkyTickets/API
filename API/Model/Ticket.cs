using API.Enums;
using API.ExportClasses;
using API.InternalClasses;

namespace API.Model;

public partial class Ticket
{
public int TId { get; set; }

public int TFlight { get; set; }

public int TUser { get; set; }

public DateTime TBoughtDate { get; set; }

public ClassOfService TClass { get; set; }

public int TTotalPrice { get; set; }

public TicketStatus TStatus { get; set; }

public virtual Flight TFlightNavigation { get; set; } = null!;

public virtual User TUserNavigation { get; set; } = null!;

public ExportTicket ToExport()
{
var fullName = $"{TUserNavigation.USurname} {TUserNavigation.UName} {TUserNavigation.UPatronymic}";
var latinName = Transliteration.ToLatin(fullName);

// Airline image
string[] files = [];
if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/")))
{
files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/"));
}
string file = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == TFlightNavigation.FAirline.ToString()) ?? "";

return new()
{
TId = TId,
TFlight = TFlight,
TUser = fullName,
TUserLatin = latinName,
TBoughtDate = TBoughtDate,
TClass = Convertation.ConvertEnumToString(TClass),
TTotalPrice = TTotalPrice,
TStatus = Convertation.ConvertEnumToString(TStatus),
FAirline = TFlightNavigation.FAirlineNavigation?.AlName,
FDepartureAirport = TFlightNavigation.FDepartureAirportNavigation?.ApName,
FArrivalAirport = TFlightNavigation.FArrivalAirportNavigation?.ApName,
FDepartureTime = TFlightNavigation.FDepartureTime,
FArrivalTime = TFlightNavigation.FArrivalTime,
AirlineImage = "/airline/" + Path.GetFileName(file),
};
}
}
