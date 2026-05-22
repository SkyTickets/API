using API.ExportClasses;
using API.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengerController(PostgresContext context) : ControllerBase
    {
        private readonly PostgresContext _context = context;

        [HttpGet("GetPassengers")]
        public async Task<IActionResult> GetPassengers()
        {
            List<Passenger> passengers = await _context.Passengers.AsNoTracking().ToListAsync();

            if (passengers.Count == 0)
                return NotFound();

            return Ok(passengers.Select(p => p.ToExport()).ToList());
        }

        [HttpGet("GetPassenger/{id}")]
        public async Task<IActionResult> GetPassenger(int id)
        {
            Passenger? passenger = await _context.Passengers.AsNoTracking().FirstOrDefaultAsync(x => x.PId == id);

            if (passenger is null)
                return NotFound("Пассажир не найден");

            return Ok(passenger.ToExport());
        }

        [HttpPost("AddPassenger")]
        public async Task<IActionResult> AddPassenger([FromBody] ExportPassenger passenger)
        {
            if (await _context.Passengers.AsNoTracking().AnyAsync(x =>
                x.PPassportSerial == passenger.PPassportSerial &&
                x.PPassportNumber == passenger.PPassportNumber))
            {
                return BadRequest("Пассажир с таким паспортом уже существует");
            }

            int id = await _context.Passengers.AsNoTracking().AnyAsync()
                ? await _context.Passengers.AsNoTracking().MaxAsync(x => x.PId) + 1
                : 1;

            Passenger newPassenger = new()
            {
                PId = id,
                PSurname = passenger.PSurname,
                PName = passenger.PName,
                PPatronymic = passenger.PPatronymic,
                PBirthdate = passenger.PBirthdate,
                PPassportSerial = passenger.PPassportSerial,
                PPassportNumber = passenger.PPassportNumber,
            };

            _context.Passengers.Add(newPassenger);
            await _context.SaveChangesAsync();

            return Ok(newPassenger.ToExport());
        }

        [HttpPost("EditPassenger")]
        public async Task<IActionResult> EditPassenger([FromBody] ExportPassenger passenger)
        {
            Passenger? gotten = await _context.Passengers.AsNoTracking().FirstOrDefaultAsync(x => x.PId == passenger.PId);

            if (gotten is null)
                return NotFound("Пассажир не найден");

            gotten.PSurname = passenger.PSurname;
            gotten.PName = passenger.PName;
            gotten.PPatronymic = passenger.PPatronymic;
            gotten.PBirthdate = passenger.PBirthdate;
            gotten.PPassportSerial = passenger.PPassportSerial;
            gotten.PPassportNumber = passenger.PPassportNumber;

            _context.Passengers.Update(gotten);
            await _context.SaveChangesAsync();

            return Ok(gotten.ToExport());
        }
    }
}
