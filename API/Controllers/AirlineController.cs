using API.ExportClasses;
using API.InternalClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirlineController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        [HttpGet("GetAirlines")]
        public async Task<IActionResult> GetAirlines()
        {
            List<Airline> airlines = await _context.Airlines.AsNoTracking().ToListAsync();

            if (airlines is null || airlines.Count == 0)
            {
                return NotFound();
            }

            List<ExportAirline> response = [];

            airlines.ForEach(airline => response.Add(airline.ToExport()));

            return Ok(response);
        }

        [HttpGet("GetAirline/{id}")]
        public async Task<IActionResult> GetAirline(int id)
        {
            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlId == id);

            if (airline is null)
            {
                return NotFound("Указанная авиакомпания не найдена");
            }

            return Ok(airline.ToExport());
        }

        [HttpPost("AddAirline")]
        public async Task<IActionResult> AddAirline([FromBody] ExportAirline airline)
        {
            Airline? gottenAirline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlName == airline.AlName);

            if (gottenAirline is not null)
            {
                return BadRequest("Авиакомпания с такими параметрами уже существует");
            }

            int id = await _context.Airlines.AsNoTracking().AnyAsync() ? await _context.Airlines.AsNoTracking().MaxAsync(x => x.AlId) + 1 : 1;

            Airline newAirline = new()
            {
                AlId = id,
                AlName = airline.AlName,
                AlEmail = airline.AlEmail,
            };

            _context.Airlines.Add(newAirline);

            await _context.SaveChangesAsync();

            return Ok(newAirline.ToExport());
        }

        [HttpPost("EditAirline")]
        public async Task<IActionResult> EditAirline([FromBody] ExportAirline airline)
        {
            Airline? gottenAirline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlId == airline.AlId);

            if (gottenAirline is null)
            {
                return BadRequest("Указанная авиакомпания не найдена");
            }

            gottenAirline.AlName = airline.AlName;
            gottenAirline.AlEmail = airline.AlEmail;

            _context.Airlines.Update(gottenAirline);

            await _context.SaveChangesAsync();

            return Ok(gottenAirline.ToExport());
        }

        [HttpDelete("DeleteAirline/{id}")]
        public async Task<IActionResult> DeleteAirline(int id)
        {
            Airline? airline = await _context.Airlines.AsNoTracking().FirstOrDefaultAsync(x => x.AlId == id);

            if (airline is null)
            {
                return NotFound("Указанная авиакомпания не найдена");
            }

            _context.Airlines.Remove(airline);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("UploadAirlineImage")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadAirlineImage([FromForm] UploadFile file, [FromForm] int airlineId)
        {
            if (file.File is null || file.File.Length == 0)
            {
                return BadRequest("Файл изображения не передан или пустой.");
            }

            if (!await _context.Airlines.AsNoTracking().AnyAsync(x => x.AlId == airlineId))
            {
                return NotFound("Указанная авиакомпания не найдена");
            }

            string airlinePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/airline/");

            if (!Directory.Exists(airlinePath))
            {
                Directory.CreateDirectory(airlinePath);
            }

            var path = Path.Combine(airlinePath, $"{airlineId}{Path.GetExtension(file.File.FileName)}");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.File.CopyToAsync(stream);
            }

            return Ok();
        }
    }
}
