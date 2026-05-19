using API.ExportClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class AirportController(PostgresContext context) : ControllerBase
{
private readonly PostgresContext _context = context;

[HttpGet("GetAirports")]
public async Task<IActionResult> GetAirports()
{
List<Airport> airports = await _context.Airports.AsNoTracking().ToListAsync();

if (airports is null || airports.Count == 0)
{
return NotFound();
}

List<ExportAirport> response = [];

airports.ForEach(airport => response.Add(airport.ToExport()));

return Ok(response);
}

[HttpGet("GetAirport/{id}")]
public async Task<IActionResult> GetAiport(int id)
{
Airport? airport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApId == id);

if (airport is null)
{
return NotFound("Указанный аэропорт не найден");
}

return Ok(airport.ToExport());
}

[HttpPost("AddAirport")]
public async Task<IActionResult> AddAirport([FromBody] ExportAirport airport)
{
Airport? gottenAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApName == airport.ApName &&
x.ApCountry == airport.ApCountry && x.ApCity == airport.ApCity && x.ApStreet == airport.ApStreet &&
x.ApBuilding == airport.ApBuilding);

if (gottenAirport is not null)
{
return BadRequest("Аэропорт с такими параметрами уже существует");
}

int id = await _context.Airports.AsNoTracking().AnyAsync() ? await _context.Airports.AsNoTracking().MaxAsync(x => x.ApId) + 1 : 1;

Airport newAirport = new()
{
ApId = id,
ApName = airport.ApName,
ApCountry = airport.ApCountry,
ApCity = airport.ApCity,
ApBuilding = airport.ApBuilding,
ApStreet = airport.ApStreet,
};

_context.Airports.Add(newAirport);

await _context.SaveChangesAsync();

return Ok(newAirport.ToExport());
}

[HttpPost("EditAirport")]
public async Task<IActionResult> EditAirport([FromBody] ExportAirport airport)
{
Airport? gottenAirport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApId == airport.ApId);

if (gottenAirport is null)
{
return BadRequest("Указанный аэропорт не найден");
}

gottenAirport.ApName = airport.ApName;
gottenAirport.ApCountry = airport.ApCountry;
gottenAirport.ApCity = airport.ApCity;
gottenAirport.ApStreet = airport.ApStreet;
gottenAirport.ApBuilding = airport.ApBuilding;

_context.Airports.Update(gottenAirport);

await _context.SaveChangesAsync();

return Ok(gottenAirport.ToExport());
}

[HttpDelete("DeleteAirport/{id}")]
public async Task<IActionResult> DeleteAirport(int id)
{
Airport? airport = await _context.Airports.AsNoTracking().FirstOrDefaultAsync(x => x.ApId == id);

if (airport is null)
{
return NotFound("Указанный аэропорт не найден");
}

_context.Airports.Remove(airport);

await _context.SaveChangesAsync();

return Ok();
}
}
}
