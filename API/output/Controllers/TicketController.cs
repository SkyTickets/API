using API.ExportClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        private IQueryable<Ticket> TicketsWithIncludes() =>
            _context.Tickets
                .AsNoTracking()
                .Include(t => t.TPassengerNavigation)
                .Include(t => t.TsServices)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FAirlineNavigation)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FDepartureAirportNavigation)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                    .ThenInclude(f => f.FArrivalAirportNavigation);

        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets()
        {
            List<Ticket> tickets = await TicketsWithIncludes().ToListAsync();

            if (tickets.Count == 0)
                return NotFound("Билеты не найдены");

            return Ok(tickets.Select(t => t.ToExport()).ToList());
        }

        [HttpGet("GetTicket/{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            Ticket? ticket = await TicketsWithIncludes().FirstOrDefaultAsync(t => t.TId == id);

            if (ticket is null)
                return NotFound("Билет не найден");

            return Ok(ticket.ToExport());
        }

        [HttpGet("GetBookingTickets/{bookingId}")]
        public async Task<IActionResult> GetBookingTickets(int bookingId)
        {
            List<Ticket> tickets = await TicketsWithIncludes()
                .Where(t => t.TBooking == bookingId)
                .ToListAsync();

            if (tickets.Count == 0)
                return NotFound("Билеты не найдены");

            return Ok(tickets.Select(t => t.ToExport()).ToList());
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromBody] TicketServiceRequest request)
        {
            Ticket? ticket = await _context.Tickets
                .Include(t => t.TsServices)
                .FirstOrDefaultAsync(t => t.TId == request.TicketId);

            if (ticket is null)
                return NotFound("Билет не найден");

            AdditionalService? service = await _context.AdditionalServices
                .FirstOrDefaultAsync(s => s.AsId == request.ServiceId);

            if (service is null)
                return NotFound("Услуга не найдена");

            if (ticket.TsServices.Any(s => s.AsId == request.ServiceId))
                return BadRequest("Услуга уже добавлена к этому билету");

            ticket.TsServices.Add(service);
            await _context.SaveChangesAsync();

            return Ok();
        }
        
        [HttpPost("RemoveService")]
        public async Task<IActionResult> RemoveService([FromBody] TicketServiceRequest request)
        {
            Ticket? ticket = await _context.Tickets
                .Include(t => t.TsServices)
                .FirstOrDefaultAsync(t => t.TId == request.TicketId);

            if (ticket is null)
                return NotFound("Билет не найден");

            AdditionalService? service = ticket.TsServices.FirstOrDefault(s => s.AsId == request.ServiceId);

            if (service is null)
                return NotFound("Услуга не найдена у данного билета");

            ticket.TsServices.Remove(service);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class TicketServiceRequest
    {
        public int TicketId { get; set; }
        public int ServiceId { get; set; }
    }
}
