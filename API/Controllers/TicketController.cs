using Microsoft.AspNetCore.Mvc;
using API.InternalClasses;
using API.ExportClasses;
using API.Model;
using API.Enums;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class TicketController(PostgresContext context) : ControllerBase
{
private readonly PostgresContext _context = context;

[HttpGet("GetTickets")]
public async Task<IActionResult> GetTickets()
{
List<Ticket> tickets = await _context.Tickets.AsNoTracking()
.Include(t => t.TUserNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FAirlineNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FDepartureAirportNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FArrivalAirportNavigation)
.ToListAsync();

if (tickets is null || tickets.Count == 0)
{
return NotFound("Билеты не найдены");
}

List<ExportTicket> response = [];

tickets.ForEach(ticket => response.Add(ticket.ToExport()));

return Ok(response);
}

[HttpGet("GetUserTickets/{id}")]
public async Task<IActionResult> GetUserTickets(int id)
{
List<Ticket> userTickets = await _context.Tickets.AsNoTracking()
.Include(t => t.TUserNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FAirlineNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FDepartureAirportNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FArrivalAirportNavigation)
.Where(x => x.TUser == id).ToListAsync();

if (userTickets is null || userTickets.Count == 0)
{
return NotFound("Билеты не найдены");
}

List<ExportTicket> response = [];

userTickets.ForEach(x => response.Add(x.ToExport()));

return Ok(response);
}

[HttpGet("GetTicket/{id}")]
public async Task<IActionResult> GetTicket(int id)
{
Ticket? ticket = await _context.Tickets.AsNoTracking()
.Include(t => t.TUserNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FAirlineNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FDepartureAirportNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FArrivalAirportNavigation)
.FirstOrDefaultAsync(x => x.TId == id);

if (ticket is null)
{
return NotFound("Билет не найден");
}

return Ok(ticket.ToExport());
}

[HttpPost("AddTicket")]
public async Task<IActionResult> AddTicket([FromBody] ExportTicket ticket)
{
Flight? flight = await _context.Flights.AsNoTracking().FirstOrDefaultAsync(x => x.FId == ticket.TFlight);

if (flight is null)
{
return BadRequest("Указанный рейс не найден");
}

List<User> users = await _context.Users.AsNoTracking().ToListAsync();

User? user = users.FirstOrDefault(x => x.UId == x.GetUserId(ticket.TUser));

if (user is null)
{
return BadRequest("Указанный пользователь не найден");
}

List<Ticket> allTicketsOnFlight = await _context.Tickets.AsNoTracking().Where(x => x.TFlight == flight.FId).ToListAsync();

if (allTicketsOnFlight.Count + 1 >= flight.FSeatsCount)
{
return BadRequest("Свободных мест нет");
}

int id = await _context.Tickets.AsNoTracking().AnyAsync() ? await _context.Tickets.AsNoTracking().MaxAsync(x => x.TId) + 1 : 1;

Ticket newTicket = new()
{
TId = id,
TFlight = flight.FId,
TUser = user.UId,
TClass = (ClassOfService)Convertation.ConvertStringToEnum<ClassOfService>(ticket.TClass)!,
TBoughtDate = DateTime.Now,
TTotalPrice = ticket.TTotalPrice,
TStatus = (TicketStatus)Convertation.ConvertStringToEnum<TicketStatus>("Куплен")!
};

_context.Tickets.Add(newTicket);

await _context.SaveChangesAsync();

await SendEmail.SendTicketAsync(_context, id);

Ticket? savedTicket = await _context.Tickets.AsNoTracking()
.Include(t => t.TUserNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FAirlineNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FDepartureAirportNavigation)
.Include(t => t.TFlightNavigation)
.ThenInclude(f => f.FArrivalAirportNavigation)
.FirstOrDefaultAsync(t => t.TId == id);

return Ok(savedTicket?.ToExport() ?? newTicket.ToExport());
}

[HttpPost("ChangeTicketStatus")]
public async Task<IActionResult> ChangeTicketStatus([FromBody] ExportTicket ticket)
{
Ticket? gottenTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.TId == ticket.TId);

if (gottenTicket is null)
{
return NotFound("Билет не найден");
}

gottenTicket.TStatus = (TicketStatus)Convertation.ConvertStringToEnum<TicketStatus>(ticket.TStatus)!;

_context.Tickets.Update(gottenTicket);

await _context.SaveChangesAsync();

return Ok(gottenTicket.ToExport());
}
}
}
