using API.ExportClasses;

namespace API.Model;

public partial class Flight
{
    public int FId { get; set; }

    public int FAirline { get; set; }

    public int FAirplane { get; set; }

    public int FDepartureAirport { get; set; }

    public int FArrivalAirport { get; set; }

    public DateTime FDepartureTime { get; set; }

    public DateTime FArrivalTime { get; set; }

    public int FBasePrice { get; set; }

    public virtual Airline FAirlineNavigation { get; set; } = null!;

    public virtual Airplane FAirplaneNavigation { get; set; } = null!;

    public virtual Airport FArrivalAirportNavigation { get; set; } = null!;

    public virtual Airport FDepartureAirportNavigation { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    // Counts tickets of a given class across non-cancelled bookings for this flight
    public int GetBookedSeats(API.Enums.ClassOfService cls)
    {
        return Bookings
            .Where(b => b.BStatus != API.Enums.BookingStatus.Отменен)
            .SelectMany(b => b.Tickets)
            .Count(t => t.TClass == cls);
    }

    public ExportFlight ToExport()
    {
        string[] files = [];
        if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/")))
        {
            files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/"));
        }

        string file = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == FAirline.ToString()) ?? "";

        var airplane = FAirplaneNavigation;

        return new()
        {
            FId = FId,
            FAirline = FAirlineNavigation.AlName,
            FAirplane = airplane?.PlModel,
            FDepartureAirport = FDepartureAirportNavigation.ApName,
            FArrivalAirport = FArrivalAirportNavigation.ApName,
            FDepartureTime = FDepartureTime,
            FArrivalTime = FArrivalTime,
            FBasePrice = FBasePrice,
            FEconomySeats = airplane?.PlEconomySeats ?? 0,
            FComfortSeats = airplane?.PlComfortSeats ?? 0,
            FBusinessSeats = airplane?.PlBusinessSeats ?? 0,
            FFirstClassSeats = airplane?.PlFirstClassSeats ?? 0,
            FAvailableEconomySeats = (airplane?.PlEconomySeats ?? 0) - GetBookedSeats(API.Enums.ClassOfService.Эконом),
            FAvailableComfortSeats = (airplane?.PlComfortSeats ?? 0) - GetBookedSeats(API.Enums.ClassOfService.Комфорт),
            FAvailableBusinessSeats = (airplane?.PlBusinessSeats ?? 0) - GetBookedSeats(API.Enums.ClassOfService.Бизнес),
            FAvailableFirstClassSeats = (airplane?.PlFirstClassSeats ?? 0) - GetBookedSeats(API.Enums.ClassOfService.Первый_класс),
            AirlineImage = "/airline/" + Path.GetFileName(file),
        };
    }
}
