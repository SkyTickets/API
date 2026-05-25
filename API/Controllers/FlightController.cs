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
                    .ThenInclude(b => b.Tickets).OrderBy(f => f.FDepartureTime);

        [HttpGet("GetFlights")]
        public async Task<IActionResult> GetFlights([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            int totalCount = await _context.Flights.CountAsync();

            var flights = await FlightsWithIncludes()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.ToExport())
                .ToListAsync();

            return Ok(new
            {
                items = flights,
                totalCount = totalCount,
                page,
                pageSize,
                hasMore = page * pageSize < totalCount
            });
        }

        [HttpGet("GetCurrentFlights")]
        public async Task<IActionResult> GetCurrentFlights([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            List<Flight> flights = await FlightsWithIncludes()
                .Where(f => f.FDepartureTime >= DateTime.Now)
                .ToListAsync();

            // Оставляем только рейсы, у которых есть хотя бы одно свободное место
            flights = flights.Where(f =>
            {
                var ap = f.FAirplaneNavigation;
                if (ap is null) return false;
                return f.GetBookedSeats(ClassOfService.Эконом) < ap.PlEconomySeats
                    || f.GetBookedSeats(ClassOfService.Комфорт) < ap.PlComfortSeats
                    || f.GetBookedSeats(ClassOfService.Бизнес) < ap.PlBusinessSeats
                    || f.GetBookedSeats(ClassOfService.Первый_класс) < ap.PlFirstClassSeats;
            }).ToList();

            int totalCount = flights.Count;
            var paged = flights
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.ToExport())
                .ToList();

            if (paged.Count == 0 && page == 1)
                return NotFound();

            return Ok(new { items = paged, totalCount, page, pageSize, hasMore = page * pageSize < totalCount });
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

            Flight newFlight = new()
            {
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

            Flight? saved = await FlightsWithIncludes().FirstOrDefaultAsync(f => f.FId == newFlight.FId);
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
                var startDateTime = parameters.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(f => f.FDepartureTime >= startDateTime);
            }

            if (parameters.EndDate.HasValue)
            {
                var endDateTime = parameters.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(f => f.FDepartureTime <= endDateTime);
            }

            if (parameters.MinCost > 0)
                query = query.Where(f => f.FBasePrice >= parameters.MinCost);

            if (parameters.MaxCost > 0)
                query = query.Where(f => f.FBasePrice <= parameters.MaxCost);

            if (!string.IsNullOrWhiteSpace(parameters.Airline))
                query = query.Where(f => f.FAirlineNavigation.AlName == parameters.Airline);

            var flights = await query.Where(f => f.FDepartureTime >= DateTime.Now).ToListAsync();

            flights = flights.Where(f =>
            {
                var ap = f.FAirplaneNavigation;
                if (ap is null) return false;

                int needed = parameters.Passengers;

                if (!String.IsNullOrEmpty(parameters.ClassOfServiceStr))
                {
                    ClassOfService? cls = null;
                    if (!string.IsNullOrWhiteSpace(parameters.ClassOfServiceStr))
                    {
                        // Заменяем пробел на подчёркивание для совпадения с именем enum
                        var enumStr = parameters.ClassOfServiceStr.Replace(" ", "_");
                        cls = Enum.Parse<ClassOfService>(enumStr);
                    }
                    int total = cls switch
                    {
                        ClassOfService.Эконом => ap.PlEconomySeats,
                        ClassOfService.Комфорт => ap.PlComfortSeats,
                        ClassOfService.Бизнес => ap.PlBusinessSeats,
                        ClassOfService.Первый_класс => ap.PlFirstClassSeats,
                        _ => 0
                    };
                    return total - f.GetBookedSeats(cls.Value) >= needed;
                }

                return f.GetBookedSeats(ClassOfService.Эконом) < ap.PlEconomySeats
                    || f.GetBookedSeats(ClassOfService.Комфорт) < ap.PlComfortSeats
                    || f.GetBookedSeats(ClassOfService.Бизнес) < ap.PlBusinessSeats
                    || f.GetBookedSeats(ClassOfService.Первый_класс) < ap.PlFirstClassSeats;
            }).ToList();

            int totalCount = flights.Count;
            var paged = flights
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(f => f.ToExport())
                .ToList();

            if (paged.Count == 0 && parameters.Page == 1)
                return NotFound("Рейсы по заданным параметрам не найдены");

            return Ok(new { items = paged, totalCount, page = parameters.Page, pageSize = parameters.PageSize, hasMore = parameters.Page * parameters.PageSize < totalCount });
        }
    }
}
