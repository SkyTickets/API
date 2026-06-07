using API.ExportClasses;
using API.Enums;
using API.InternalClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        private IQueryable<Flight> FlightsWithIncludes() =>
            _context.Flights
                .AsNoTracking()
                .Include(f => f.FAirlineNavigation)
                .Include(f => f.FAirplaneNavigation)
                .Include(f => f.FDepartureAirportNavigation)
                .Include(f => f.FArrivalAirportNavigation)
                .Include(f => f.Bookings.Where(b => b.BStatus != BookingStatus.Отменен))
                    .ThenInclude(b => b.Tickets);

        [HttpGet("GetFlights")]
        public async Task<IActionResult> GetFlights()
        {
            List<Flight> flights = await FlightsWithIncludes().ToListAsync();

            if (flights.Count == 0)
                return NotFound();

            return Ok(flights.Select(f => f.ToExport()).ToList());
        }

        [HttpGet("GetCurrentFlights")]
        public async Task<IActionResult> GetCurrentFlights()
        {
            List<Flight> flights = await FlightsWithIncludes()
                .Where(f => f.FDepartureTime >= DateTime.UtcNow)
                .ToListAsync();

            flights = flights.Where(f =>
            {
                var ap = f.FAirplaneNavigation;
                if (ap is null) return false;
                return f.GetBookedSeats(ClassOfService.Эконом) < ap.PlEconomySeats
                    || f.GetBookedSeats(ClassOfService.Комфорт) < ap.PlComfortSeats
                    || f.GetBookedSeats(ClassOfService.Бизнес) < ap.PlBusinessSeats
                    || f.GetBookedSeats(ClassOfService.Первый_класс) < ap.PlFirstClassSeats;
            }).ToList();

            if (flights.Count == 0)
                return NotFound();

            return Ok(flights.Select(f => f.ToExport()).ToList());
        }

        [HttpGet("GetFlight/{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            Flight? flight = await FlightsWithIncludes().FirstOrDefaultAsync(x => x.FId == id);

            if (flight is null)
                return NotFound("Указанный рейс не найден");

            return Ok(flight.ToExport());
        }

        [HttpPost("AddFlight")]
        public async Task<IActionResult> AddFlight([FromBody] ExportFlight flight)
        {
            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlName == flight.FAirline);
            Airport? departureAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FDepartureAirport);
            Airport? arrivalAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FArrivalAirport);
            Airplane? airplane = await _context.Airplanes.AsNoTracking().FirstOrDefaultAsync(x => x.PlModel == flight.FAirplane);

            if (airline is null)
                return BadRequest("Указанная авиакомпания не найдена");
            if (departureAirport is null)
                return BadRequest("Указанный аэропорт отправления не найден");
            if (arrivalAirport is null)
                return BadRequest("Указанный аэропорт прибытия не найден");
            if (airplane is null)
                return BadRequest("Указанный самолёт не найден");

            bool duplicate = await _context.Flights.AsNoTracking().AnyAsync(x =>
                x.FAirline == airline.AlId &&
                x.FArrivalAirport == arrivalAirport.ApId &&
                x.FDepartureAirport == departureAirport.ApId &&
                x.FDepartureTime == flight.FDepartureTime &&
                x.FArrivalTime == flight.FArrivalTime);

            if (duplicate)
                return BadRequest("Рейс с такими параметрами уже существует");

            int id = await _context.Flights.AsNoTracking().AnyAsync()
                ? await _context.Flights.AsNoTracking().MaxAsync(x => x.FId) + 1
                : 1;

            Flight newFlight = new()
            {
                FId = id,
                FAirline = airline.AlId,
                FAirplane = airplane.PlId,
                FDepartureAirport = departureAirport.ApId,
                FArrivalAirport = arrivalAirport.ApId,
                FDepartureTime = flight.FDepartureTime,
                FArrivalTime = flight.FArrivalTime,
                FBasePrice = flight.FBasePrice,
            };

            _context.Flights.Add(newFlight);
            await _context.SaveChangesAsync();

            Flight? saved = await FlightsWithIncludes().FirstOrDefaultAsync(f => f.FId == id);
            return Ok(saved?.ToExport() ?? newFlight.ToExport());
        }

        [HttpPost("EditFlight")]
        public async Task<IActionResult> EditFlight([FromBody] ExportFlight flight)
        {
            Flight? gottenFlight = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.FId == flight.FId);

            if (gottenFlight is null)
                return NotFound("Указанный рейс не найден");

            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlName == flight.FAirline);
            Airport? departureAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FDepartureAirport);
            Airport? arrivalAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FArrivalAirport);
            Airplane? airplane = await _context.Airplanes.AsNoTracking().FirstOrDefaultAsync(x => x.PlModel == flight.FAirplane);

            if (airline is null) return BadRequest("Указанная авиакомпания не найдена");
            if (departureAirport is null) return BadRequest("Указанный аэропорт отправления не найден");
            if (arrivalAirport is null) return BadRequest("Указанный аэропорт прибытия не найден");
            if (airplane is null) return BadRequest("Указанный самолёт не найден");

            gottenFlight.FAirline = airline.AlId;
            gottenFlight.FAirplane = airplane.PlId;
            gottenFlight.FArrivalAirport = arrivalAirport.ApId;
            gottenFlight.FDepartureAirport = departureAirport.ApId;
            gottenFlight.FDepartureTime = flight.FDepartureTime;
            gottenFlight.FArrivalTime = flight.FArrivalTime;
            gottenFlight.FBasePrice = flight.FBasePrice;

            _context.Flights.Update(gottenFlight);
            await _context.SaveChangesAsync();

            Flight? saved = await FlightsWithIncludes().FirstOrDefaultAsync(f => f.FId == gottenFlight.FId);
            return Ok(saved?.ToExport() ?? gottenFlight.ToExport());
        }

        [HttpPost("SearchFlights")]
        public async Task<IActionResult> SearchFlights([FromBody] SearchFlightParams parameters)
        {
            var query = FlightsWithIncludes();

            if (!string.IsNullOrWhiteSpace(parameters.CityFrom))
                query = query.Where(f => f.FDepartureAirportNavigation.ApCity == parameters.CityFrom);

            if (!string.IsNullOrWhiteSpace(parameters.CityTo))
                query = query.Where(f => f.FArrivalAirportNavigation.ApCity == parameters.CityTo);

            if (parameters.StartDate.HasValue)
            {
                var startDateTime = parameters.StartDate.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
                query = query.Where(f => f.FDepartureTime >= startDateTime);
            }

            if (parameters.EndDate.HasValue)
            {
                var endDateTime = parameters.EndDate.Value.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();
                query = query.Where(f => f.FDepartureTime <= endDateTime);
            }

            if (parameters.MinCost > 0)
                query = query.Where(f => f.FBasePrice >= parameters.MinCost);

            if (parameters.MaxCost > 0)
                query = query.Where(f => f.FBasePrice <= parameters.MaxCost);

            if (!string.IsNullOrWhiteSpace(parameters.Airline))
                query = query.Where(f => f.FAirlineNavigation.AlName == parameters.Airline);

            var flights = await query.Where(f => f.FDepartureTime >= DateTime.UtcNow).ToListAsync();

            flights = flights.Where(f =>
            {
                var ap = f.FAirplaneNavigation;
                if (ap is null) return false;
                return f.GetBookedSeats(ClassOfService.Эконом) < ap.PlEconomySeats
                    || f.GetBookedSeats(ClassOfService.Комфорт) < ap.PlComfortSeats
                    || f.GetBookedSeats(ClassOfService.Бизнес) < ap.PlBusinessSeats
                    || f.GetBookedSeats(ClassOfService.Первый_класс) < ap.PlFirstClassSeats;
            }).ToList();

            if (flights.Count == 0)
                return NotFound("Рейсы по заданным параметрам не найдены");

            return Ok(flights.Select(f => f.ToExport()).ToList());
        }
    }
}
