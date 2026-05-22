using API.ExportClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionalServiceController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        [HttpGet("GetServices")]
        public async Task<IActionResult> GetServices()
        {
            List<AdditionalService> services = await _context.AdditionalServices.AsNoTracking().ToListAsync();

            if (services.Count == 0)
                return NotFound();

            return Ok(services.Select(s => s.ToExport()).ToList());
        }

        [HttpGet("GetService/{id}")]
        public async Task<IActionResult> GetService(int id)
        {
            AdditionalService? service = await _context.AdditionalServices.AsNoTracking().FirstOrDefaultAsync(s => s.AsId == id);

            if (service is null)
                return NotFound("Услуга не найдена");

            return Ok(service.ToExport());
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromBody] ExportAdditionalService service)
        {
            if (await _context.AdditionalServices.AsNoTracking().AnyAsync(s => s.AsName == service.AsName))
                return BadRequest("Услуга с таким названием уже существует");

            int id = await _context.AdditionalServices.AsNoTracking().AnyAsync()
                ? await _context.AdditionalServices.AsNoTracking().MaxAsync(s => s.AsId) + 1
                : 1;

            AdditionalService newService = new()
            {
                AsId = id,
                AsName = service.AsName,
                AsPrice = service.AsPrice,
            };

            _context.AdditionalServices.Add(newService);
            await _context.SaveChangesAsync();

            return Ok(newService.ToExport());
        }

        [HttpPost("EditService")]
        public async Task<IActionResult> EditService([FromBody] ExportAdditionalService service)
        {
            AdditionalService? gotten = await _context.AdditionalServices.AsNoTracking().FirstOrDefaultAsync(s => s.AsId == service.AsId);

            if (gotten is null)
                return NotFound("Услуга не найдена");

            gotten.AsName = service.AsName;
            gotten.AsPrice = service.AsPrice;

            _context.AdditionalServices.Update(gotten);
            await _context.SaveChangesAsync();

            return Ok(gotten.ToExport());
        }

        [HttpDelete("DeleteService/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            AdditionalService? service = await _context.AdditionalServices.AsNoTracking().FirstOrDefaultAsync(s => s.AsId == id);

            if (service is null)
                return NotFound("Услуга не найдена");

            _context.AdditionalServices.Remove(service);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
