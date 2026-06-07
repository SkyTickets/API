using API.Enums;
using API.ExportClasses;
using API.InternalClasses;

namespace API.Model;

public partial class Ticket
{
    public int TId { get; set; }

    public int TBooking { get; set; }

    public int TPassenger { get; set; }

    public ClassOfService TClass { get; set; }

    public int TPrice { get; set; }

    public virtual Booking TBookingNavigation { get; set; } = null!;

    public virtual Passenger TPassengerNavigation { get; set; } = null!;

    public virtual ICollection<AdditionalService> TsServices { get; set; } = new List<AdditionalService>();

    public ExportTicket ToExport()
    {
        var passenger = TPassengerNavigation;
        var fullName = passenger is not null
            ? $"{passenger.PSurname} {passenger.PName} {passenger.PPatronymic}".Trim()
            : "";
        var latinName = Transliteration.ToLatin(fullName);

        var flight = TBookingNavigation?.BFlightNavigation;

        string[] files = [];
        if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/")))
        {
            files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/"));
        }
        string file = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == flight?.FAirline.ToString()) ?? "";

        return new()
        {
            TId = TId,
            TBooking = TBooking,
            TPassenger = fullName,
            TPassengerLatin = latinName,
            TClass = Convertation.ConvertEnumToString(TClass),
            TPrice = TPrice,
            Services = TsServices.Select(s => s.ToExport()).ToList(),
            FAirline = flight?.FAirlineNavigation?.AlName,
            FDepartureAirport = flight?.FDepartureAirportNavigation?.ApName,
            FArrivalAirport = flight?.FArrivalAirportNavigation?.ApName,
            FDepartureTime = flight?.FDepartureTime,
            FArrivalTime = flight?.FArrivalTime,
            AirlineImage = "/airline/" + Path.GetFileName(file),
        };
    }
}
