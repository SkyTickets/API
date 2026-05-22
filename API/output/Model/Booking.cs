using API.Enums;
using API.ExportClasses;
using API.InternalClasses;

namespace API.Model;

public partial class Booking
{
    public int BId { get; set; }

    public int BUser { get; set; }

    public int BFlight { get; set; }

    public DateTime BCreatedAt { get; set; }

    public BookingStatus BStatus { get; set; }

    public int BTotalPrice { get; set; }

    public virtual User BUserNavigation { get; set; } = null!;

    public virtual Flight BFlightNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ExportBooking ToExport()
    {
        return new()
        {
            BId = BId,
            BUser = BUser,
            BFlight = BFlight,
            BCreatedAt = BCreatedAt,
            BStatus = Convertation.ConvertEnumToString(BStatus),
            BTotalPrice = BTotalPrice,
            BUserName = BUserNavigation is not null
                ? $"{BUserNavigation.USurname} {BUserNavigation.UName} {BUserNavigation.UPatronymic}".Trim()
                : null,
            FAirline = BFlightNavigation?.FAirlineNavigation?.AlName,
            FDepartureAirport = BFlightNavigation?.FDepartureAirportNavigation?.ApName,
            FArrivalAirport = BFlightNavigation?.FArrivalAirportNavigation?.ApName,
            FDepartureTime = BFlightNavigation?.FDepartureTime,
            FArrivalTime = BFlightNavigation?.FArrivalTime,
            Tickets = Tickets.Select(t => t.ToExport()).ToList(),
        };
    }
}
