using API.Enums;
using API.ExportClasses;
using API.InternalClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController(PostgresContext context, IDbContextFactory<PostgresContext> contextFactory) : ControllerBase
    {
        private readonly PostgresContext _context = context;
        private readonly IDbContextFactory<PostgresContext> _contextFactory = contextFactory;


        private IQueryable<Booking> BookingsWithIncludes() =>
            _context.Bookings
                .AsNoTracking()
                .Include(b => b.BUserNavigation)
                .Include(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FAirlineNavigation)
                .Include(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FDepartureAirportNavigation)
                .Include(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FArrivalAirportNavigation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.TPassengerNavigation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.TsServices);   

        [HttpGet("GetBookings")]
        public async Task<IActionResult> GetBookings()
        {
            List<Booking> bookings = await BookingsWithIncludes().ToListAsync();

            if (bookings.Count == 0)
                return NotFound("Бронирования не найдены");

            return Ok(bookings.Select(b => b.ToExport()).ToList());
        }

        [HttpGet("GetUserBookings/{userId}")]
        public async Task<IActionResult> GetUserBookings(int userId)
        {
            List<Booking> bookings = await BookingsWithIncludes()
                .Where(b => b.BUser == userId)
                .ToListAsync();

            if (bookings.Count == 0)
                return NotFound("Бронирования не найдены");

            return Ok(bookings.Select(b => b.ToExport()).ToList());
        }

        [HttpGet("GetBooking/{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            Booking? booking = await BookingsWithIncludes().FirstOrDefaultAsync(b => b.BId == id);

            if (booking is null)
                return NotFound("Бронирование не найдено");

            return Ok(booking.ToExport());
        }

        [HttpPost("AddBooking")]
        public async Task<IActionResult> AddBooking([FromBody] CreateBookingRequest request)
        {
            if (request.Tickets is null || request.Tickets.Count == 0)
                return BadRequest("Необходимо указать хотя бы один билет");

            Flight? flight = await _context.Flights
                .AsNoTracking()
                .Include(f => f.FAirplaneNavigation)
                .Include(f => f.Bookings.Where(b => b.BStatus != BookingStatus.Отменен))
                    .ThenInclude(b => b.Tickets)
                .FirstOrDefaultAsync(f => f.FId == request.BFlight);

            if (flight is null)
                return BadRequest("Указанный рейс не найден");

            if (!await _context.Users.AsNoTracking().AnyAsync(u => u.UId == request.BUser))
                return BadRequest("Указанный пользователь не найден");

            foreach (var ticketReq in request.Tickets)
            {
                var cls = (ClassOfService)Convertation.ConvertStringToEnum<ClassOfService>(ticketReq.TClass)!;
                int capacity = cls switch
                {
                    ClassOfService.Эконом        => flight.FAirplaneNavigation?.PlEconomySeats    ?? 0,
                    ClassOfService.Комфорт       => flight.FAirplaneNavigation?.PlComfortSeats    ?? 0,
                    ClassOfService.Бизнес        => flight.FAirplaneNavigation?.PlBusinessSeats   ?? 0,
                    ClassOfService.Первый_класс  => flight.FAirplaneNavigation?.PlFirstClassSeats ?? 0,
                    _ => 0
                };
                if (flight.GetBookedSeats(cls) >= capacity)
                    return BadRequest($"Нет свободных мест класса «{ticketReq.TClass}»");
            }

            foreach (var ticketReq in request.Tickets)
            {
                if (!await _context.Passengers.AsNoTracking().AnyAsync(p => p.PId == ticketReq.TPassengerId))
                    return BadRequest($"Пассажир с id {ticketReq.TPassengerId} не найден");
            }

            var status = (BookingStatus)Convertation.ConvertStringToEnum<BookingStatus>(request.BStatus ?? "Забронирован")!;

            Booking newBooking = new()
            {
                BUser = request.BUser,
                BFlight = request.BFlight,
                BCreatedAt = DateTime.Now,
                BStatus = status,
                BTotalPrice = request.BTotalPrice,
            };

            _context.Bookings.Add(newBooking);

            var ticketsToCreate = request.Tickets.Select(ticketReq => new Ticket
            {
                TBookingNavigation = newBooking,
                TPassenger = ticketReq.TPassengerId,
                TClass = (ClassOfService)Convertation.ConvertStringToEnum<ClassOfService>(ticketReq.TClass)!,
                TPrice = ticketReq.TPrice,
            }).ToList();

            _context.Tickets.AddRange(ticketsToCreate);
            await _context.SaveChangesAsync();

            for (int i = 0; i < ticketsToCreate.Count; i++)
            {
                var serviceIds = request.Tickets[i].ServiceIds;
                if (serviceIds is null || serviceIds.Count == 0) continue;

                Ticket? savedTicket = await _context.Tickets
                    .Include(t => t.TsServices)
                    .FirstOrDefaultAsync(t => t.TId == ticketsToCreate[i].TId);

                if (savedTicket is null) continue;

                foreach (int serviceId in serviceIds)
                {
                    AdditionalService? svc = await _context.AdditionalServices
                        .FirstOrDefaultAsync(s => s.AsId == serviceId);

                    if (svc is not null)
                        savedTicket.TsServices.Add(svc);
                }
            }

            await _context.SaveChangesAsync();

            Booking? saved = await BookingsWithIncludes().FirstOrDefaultAsync(b => b.BId == newBooking.BId);
            return Ok(saved?.ToExport());
        }

        [HttpPost("ChangeBookingStatus")]
        public async Task<IActionResult> ChangeBookingStatus([FromBody] ExportBooking booking)
        {
            Booking? gotten = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BId == booking.BId);

            if (gotten is null)
                return NotFound("Бронирование не найдено");

            gotten.BStatus = (BookingStatus)Convertation.ConvertStringToEnum<BookingStatus>(booking.BStatus)!;

            _context.Bookings.Update(gotten);
            await _context.SaveChangesAsync();

            Booking? saved = await BookingsWithIncludes().FirstOrDefaultAsync(b => b.BId == gotten.BId);
            return Ok(saved?.ToExport());
        }

        [HttpPost("PayBooking")]
        public async Task<IActionResult> PayBooking([FromBody] PayBookingRequest request)
        {
            Booking? booking = await _context.Bookings
                .Include(b => b.Tickets).ThenInclude(t => t.TsServices)
                .Include(b => b.Tickets).ThenInclude(t => t.TPassengerNavigation)
                .Include(b => b.BFlightNavigation).ThenInclude(f => f.FAirlineNavigation)
                .Include(b => b.BFlightNavigation).ThenInclude(f => f.FDepartureAirportNavigation)
                .Include(b => b.BFlightNavigation).ThenInclude(f => f.FArrivalAirportNavigation)
                .Include(b => b.BUserNavigation)
                .FirstOrDefaultAsync(b => b.BId == request.BId);

            if (booking is null) return NotFound("Бронирование не найдено");
            if (booking.BStatus == BookingStatus.Оплачен) return BadRequest("Бронирование уже оплачено");
            if (booking.BStatus == BookingStatus.Отменен) return BadRequest("Нельзя оплатить отменённое бронирование");

            if (request.Services is not null)
            {
                foreach (var svcReq in request.Services)
                {
                    var ticket = booking.Tickets.FirstOrDefault(t => t.TId == svcReq.TicketId);
                    if (ticket is null) continue;
                    foreach (int serviceId in svcReq.ServiceIds ?? [])
                    {
                        if (ticket.TsServices.Any(s => s.AsId == serviceId)) continue;
                        AdditionalService? svc = await _context.AdditionalServices.FirstOrDefaultAsync(s => s.AsId == serviceId);
                        if (svc is not null) ticket.TsServices.Add(svc);
                    }
                }
            }

            booking.BTotalPrice = booking.Tickets.Sum(t => t.TPrice + t.TsServices.Sum(s => s.AsPrice));
            booking.BStatus = BookingStatus.Оплачен;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            var emailTasks = booking.Tickets
    .Select(t => SendEmail.SendTicketAsync(_contextFactory, t.TId))
    .ToList();

            emailTasks.Add(SendEmail.SendReceiptAsync(_contextFactory, booking.BId));

            await Task.WhenAll(emailTasks);

            Booking? saved = await BookingsWithIncludes().FirstOrDefaultAsync(b => b.BId == booking.BId);
            return Ok(saved?.ToExport());
        }

        [HttpDelete("DeleteBooking/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            Booking? booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BId == id);

            if (booking is null)
                return NotFound("Бронирование не найдено");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class CreateBookingRequest
    {
        public int BUser { get; set; }
        public int BFlight { get; set; }
        public string? BStatus { get; set; }
        public int BTotalPrice { get; set; }
        public List<CreateTicketRequest> Tickets { get; set; } = [];
    }

    public class CreateTicketRequest
    {
        public int TPassengerId { get; set; }
        public string TClass { get; set; } = null!;
        public int TPrice { get; set; }
        public List<int>? ServiceIds { get; set; }
    }

    public class PayBookingRequest
    {
        public int BId { get; set; }
        public List<TicketServicesRequest>? Services { get; set; }
    }

    public class TicketServicesRequest
    {
        public int TicketId { get; set; }
        public List<int>? ServiceIds { get; set; }
    }
}
