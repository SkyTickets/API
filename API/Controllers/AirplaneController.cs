using API.ExportClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirplaneController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        [HttpGet("GetAirplanes")]
        public async Task<IActionResult> GetAirplanes()
        {
            List<Airplane> airplanes = await _context.Airplanes.AsNoTracking().ToListAsync();

            if (airplanes.Count == 0)
                return NotFound();

            return Ok(airplanes.Select(a => a.ToExport()).ToList());
        }

        [HttpGet("GetAirplane/{id}")]
        public async Task<IActionResult> GetAirplane(int id)
        {
            Airplane? airplane = await _context.Airplanes.AsNoTracking().FirstOrDefaultAsync(x => x.PlId == id);

            if (airplane is null)
                return NotFound("Указанный самолёт не найден");

            return Ok(airplane.ToExport());
        }

        [HttpPost("AddAirplane")]
        public async Task<IActionResult> AddAirplane([FromBody] ExportAirplane airplane)
        {
            if (await _context.Airplanes.AsNoTracking().AnyAsync(x => x.PlModel == airplane.PlModel))
                return BadRequest("Самолёт с такой моделью уже существует");

            Airplane newAirplane = new()
            {
                PlModel = airplane.PlModel,
                PlEconomySeats = airplane.PlEconomySeats,
                PlComfortSeats = airplane.PlComfortSeats,
                PlBusinessSeats = airplane.PlBusinessSeats,
                PlFirstClassSeats = airplane.PlFirstClassSeats,
            };

            _context.Airplanes.Add(newAirplane);
            await _context.SaveChangesAsync();

            return Ok(newAirplane.ToExport());
        }

        [HttpPost("EditAirplane")]
        public async Task<IActionResult> EditAirplane([FromBody] ExportAirplane airplane)
        {
            Airplane? gotten = await _context.Airplanes.AsNoTracking().FirstOrDefaultAsync(x => x.PlId == airplane.PlId);

            if (gotten is null)
                return NotFound("Указанный самолёт не найден");

            gotten.PlModel = airplane.PlModel;
            gotten.PlEconomySeats = airplane.PlEconomySeats;
            gotten.PlComfortSeats = airplane.PlComfortSeats;
            gotten.PlBusinessSeats = airplane.PlBusinessSeats;
            gotten.PlFirstClassSeats = airplane.PlFirstClassSeats;

            _context.Airplanes.Update(gotten);
            await _context.SaveChangesAsync();

            return Ok(gotten.ToExport());
        }

        [HttpDelete("DeleteAirplane/{id}")]
        public async Task<IActionResult> DeleteAirplane(int id)
        {
            Airplane? airplane = await _context.Airplanes.AsNoTracking().FirstOrDefaultAsync(x => x.PlId == id);

            if (airplane is null)
                return NotFound("Указанный самолёт не найден");

            _context.Airplanes.Remove(airplane);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
