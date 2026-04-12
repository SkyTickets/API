using API.ExportClasses;
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

        [HttpGet("GetFlights")]
        public async Task<IActionResult> GetFlights()
        {
            List<Flight> flights = await _context.Flights.AsNoTracking().ToListAsync();

            if (flights is null || flights.Count == 0)
            {
                return NotFound();
            }

            List<ExportFlight> response = [];

            flights.ForEach(flight => response.Add(flight.ToExport()));
            return Ok(response);
        }

        [HttpGet("GetCurrentFlights")]
        public async Task<IActionResult> GetCurrentFlights()
        {
            List<Ticket> tickets = await _context.Tickets.AsNoTracking().ToListAsync() ?? [];
            List<Flight> flights = await _context.Flights.AsNoTracking().ToListAsync();
            flights = flights
                .Where(x => x.FDepartureTime >= DateTime.Now.ToUniversalTime()).AsEnumerable()
                .Where(x => x.FSeatsCount > tickets.AsEnumerable().Where(t => t.TFlight == x.FId).ToList().Count)
                .ToList() ?? [];

            if (flights is null || flights.Count == 0)
            {
                return NotFound();
            }

            List<ExportFlight> response = [];

            flights.ForEach(flight => response.Add(flight.ToExport()));
            return Ok(response);
        }

        [HttpGet("GetFlight/{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            Flight? flight = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.FId == id);

            if (flight is null)
            {
                return NotFound("Указанный рейс не найден");
            }

            return Ok(flight.ToExport());
        }

        [HttpPost("AddFlight")]
        public async Task<IActionResult> AddFlight([FromBody] ExportFlight flight)
        {
            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlName == flight.FAirline);
            Airport? departureAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FDepartureAirport);
            Airport? arrivalAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FArrivalAirport);

            if (airline is null)
            {
                return BadRequest("Указанная авиакомпания не найдена");
            }

            if (departureAirport is null)
            {
                return BadRequest("Указанный аэропорт страны отправления не найден");
            }

            if (arrivalAirport is null)
            {
                return BadRequest("Указанный аэропорт страны назначения не найден");
            }

            Flight? gottenFlight = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.FAirline == airline.AlId &&
            x.FArrivalAirport == arrivalAirport.ApId && x.FDepartureAirport == departureAirport.ApId &&
            x.FDepartureTime == flight.FDepartureTime && x.FArrivalTime == flight.FArrivalTime);

            if (gottenFlight is not null)
            {
                return BadRequest("Рейс с такими параметрами уже существует");
            }

            int id = await _context.Flights.AsNoTracking().AnyAsync() ? await _context.Flights.AsNoTracking().MaxAsync(x => x.FId) + 1 : 1;

            Flight newFlight = new()
            {
                FId = id,
                FAirline = airline.AlId,
                FDepartureAirport = departureAirport.ApId,
                FArrivalAirport = arrivalAirport.ApId,
                FDepartureTime = flight.FDepartureTime,
                FArrivalTime = flight.FArrivalTime,
                FSeatsCount = flight.FSeatsCount,
                FPrice = flight.FPrice,
            };

            _context.Flights.Add(newFlight);

            await _context.SaveChangesAsync();

            return Ok(newFlight.ToExport());
        }

        [HttpPost("EditFlight")]
        public async Task<IActionResult> EditFlight([FromBody] ExportFlight flight)
        {
            Flight? gottenFlight = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.FId == flight.FId);

            if (gottenFlight is null)
            {
                return NotFound("Указанный рейс не найден");
            }

            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlName == flight.FAirline);
            Airport? departutrAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FDepartureAirport);
            Airport? arrivalAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == flight.FArrivalAirport);

            if (airline is null)
            {
                return BadRequest("Указанная авиакомпания не найдена");
            }

            if (departutrAirport is null)
            {
                return BadRequest("Указанный аэропорт страны отправления не найден");
            }

            if (arrivalAirport is null)
            {
                return BadRequest("Указанный аэропорт страны назначения не найден");
            }

            gottenFlight.FAirline = airline.AlId;
            gottenFlight.FArrivalAirport = arrivalAirport.ApId;
            gottenFlight.FDepartureAirport = departutrAirport.ApId;
            gottenFlight.FDepartureTime = flight.FDepartureTime;
            gottenFlight.FArrivalTime = flight.FArrivalTime;
            gottenFlight.FSeatsCount = flight.FSeatsCount;
            gottenFlight.FPrice = flight.FPrice;

            _context.Flights.Update(gottenFlight);
            await _context.SaveChangesAsync();

            return Ok(gottenFlight.ToExport());
        }

        [HttpPost("SearchFlights")]
        public async Task<IActionResult> SearchFlights([FromBody] SearchFlightParams parameters)
        {
            var query = _context.Flights
                .AsNoTracking()
                .Include(f => f.FAirlineNavigation)
                .Include(f => f.FDepartureAirportNavigation)
                .Include(f => f.FArrivalAirportNavigation)
                .AsQueryable();

            // Фильтр по городу отправления
            if (!string.IsNullOrWhiteSpace(parameters.CityFrom))
            {
                query = query.Where(f => f.FDepartureAirportNavigation.ApCity == parameters.CityFrom);
            }

            // Фильтр по городу прибытия
            if (!string.IsNullOrWhiteSpace(parameters.CityTo))
            {
                query = query.Where(f => f.FArrivalAirportNavigation.ApCity == parameters.CityTo);
            }

            // Фильтр по дате вылета (StartDate)
            if (parameters.StartDate.HasValue)
            {
                var startDateTime = parameters.StartDate.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
                query = query.Where(f => f.FDepartureTime >= startDateTime);
            }

            // Фильтр по дате прилёта (EndDate) — до конца дня
            if (parameters.EndDate.HasValue)
            {
                var endDateTime = parameters.EndDate.Value.ToDateTime(TimeOnly.MaxValue).ToUniversalTime();
                query = query.Where(f => f.FDepartureTime <= endDateTime);
            }

            // Фильтр по минимальной цене
            if (parameters.MinCost > 0)
            {
                query = query.Where(f => f.FPrice >= parameters.MinCost);
            }

            // Фильтр по максимальной цене
            if (parameters.MaxCost > 0)
            {
                query = query.Where(f => f.FPrice <= parameters.MaxCost);
            }

            // Фильтр по авиакомпании
            if (!string.IsNullOrWhiteSpace(parameters.Airline))
            {
                query = query.Where(f => f.FAirlineNavigation.AlName == parameters.Airline);
            }

            var flights = await query.ToListAsync();

            // Бизнес-логика: только будущие рейсы с доступными местами
            var tickets = await _context.Tickets.AsNoTracking().ToListAsync() ?? new List<Ticket>();

            flights = flights
                .Where(f => f.FDepartureTime >= DateTime.UtcNow)
                .Where(f => f.FSeatsCount > tickets.Count(t => t.TFlight == f.FId))
                .ToList();

            if (flights.Count == 0)
            {
                return NotFound("Рейсы по заданным параметрам не найдены");
            }

            var response = flights.Select(f => f.ToExport()).ToList();

            return Ok(response);
        }
    }
}
