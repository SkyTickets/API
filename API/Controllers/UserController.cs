using API.Enums;
using API.ExportClasses;
using API.InternalClasses;
using API.Model;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class UserController(PostgresContext context) : ControllerBase
{
private readonly PostgresContext _context = context;

[HttpGet("GetUsers")]
public async Task<IActionResult> GetUsers()
{
List<User> users = await _context.Users.AsNoTracking().ToListAsync();

if (users is null || users.Count == 0)
{
return NotFound();
}
List<ExportUser> response = [];

users.ForEach(x => response.Add(x.ToExport()));

return Ok(response);
}

[HttpGet("GetUser/{id}")]
public async Task<IActionResult> GetUser(int id)
{
User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UId == id);

if (user is null)
{
return NotFound("Пользователь не найден");
}

return Ok(user.ToExport());
}

[HttpDelete("DeleteUser/{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UId == id);

if (user is null)
{
return NotFound("Пользователь не найден");
}

_context.Users.Remove(user);

await _context.SaveChangesAsync();

return Ok();
}


[HttpPost("Auth")]
public async Task<IActionResult> Authorization([FromForm] string login, [FromForm] string password)
{
User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UEmail == login);

if (user is null)
{
return BadRequest("Неверный логин или пароль");
}

PasswordHasher<ExportUser> hasher = new();

if (hasher.VerifyHashedPassword(user.ToExport(), user.UPassword, password) == PasswordVerificationResult.Success ||
user.UPassword == password)
{
return Ok(user.ToExport());
}

return BadRequest("Неверный логин или пароль");
}

[HttpPost("Register")]
public async Task<IActionResult> Register([FromBody] ExportUser user)
{
if (await _context.Users.AsNoTracking().AnyAsync(x => x.UEmail == user.UEmail))
{
return BadRequest("Указанный логин занят");
}

if (await _context.Users.AsNoTracking().AnyAsync(x => x.UPhone == user.UPhone))
{
return BadRequest("Указаный номер телефона занят");
}

if (await _context.Users.AsNoTracking().AnyAsync(x => x.UPassportNumber == user.UPassportNumber))
{
return BadRequest("Указаный номер пасспорта уже используется");
}

int id = await _context.Users.AsNoTracking().AnyAsync() ? await _context.Users.AsNoTracking().MaxAsync(x => x.UId) + 1 : 1;

if (await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UId == id) != null)
{
return BadRequest();
}

PasswordHasher<ExportUser> hasher = new();
string encPassword = hasher.HashPassword(user, user.UPassword);
User new_user = new()
{
UId = id,
UName = user.UName,
USurname = user.USurname,
UPatronymic = user.UPatronymic,
UEmail = user.UEmail,
UPassword = encPassword,
URole = (Role)Convertation.ConvertStringToEnum<Role>("Клиент")!,
UPhone = user.UPhone,
UBirthdate = user.UBirthdate,
UPassportNumber = user.UPassportNumber,
UPassportSerial = user.UPassportSerial,
};

_context.Users.Add(new_user);

await _context.SaveChangesAsync();

await SendEmail.SendLoginInformationAsync(user.UEmail, user.UPassword);


return Ok(new_user.ToExport());
}

[HttpPost("ChangePassword")]
public async Task<IActionResult> ChangeUserPassword([FromBody] ExportUser user)
{
User? gottenUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UId == user.UId);

if (gottenUser is null)
{
return BadRequest("Пользователь не найден");
}

PasswordHasher<ExportUser> hasher = new();
string newPassword = hasher.HashPassword(gottenUser.ToExport(), user.UPassword);

gottenUser.UPassword = newPassword;

_context.Users.Update(gottenUser);
await _context.SaveChangesAsync();

return Ok(gottenUser.ToExport());
}

[HttpPost("EditUser")]
public async Task<IActionResult> EditUserInfo([FromBody] ExportUser user)
{
User? gottenUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UId == user.UId);

if (gottenUser is null)
{
return BadRequest("Пользователь не найден");
}

List<string> phones = await _context.Users.Select(x => x.UPhone).ToListAsync();

phones.Remove(gottenUser.UPhone);

if (phones.Contains(user.UPhone))
{
return BadRequest("Указанный номер телеофна занят");
}

gottenUser.UName = user.UName;
gottenUser.USurname = user.USurname;
gottenUser.UPatronymic = user.UPatronymic;
gottenUser.UPhone = user.UPhone;
gottenUser.UEmail = user.UEmail;
gottenUser.UPassportSerial = user.UPassportSerial;
gottenUser.UPassportNumber = user.UPassportNumber;
gottenUser.UBirthdate = user.UBirthdate;


_context.Users.Update(gottenUser);
await _context.SaveChangesAsync();

return Ok(gottenUser.ToExport());
}

[HttpPost("UploadUserImage")]
[RequestSizeLimit(10_000_000)]
public async Task<IActionResult> UploadUserImage([FromForm] UploadFile file, [FromForm] int userId)
{
if (file.File is null || file.File.Length == 0)
{
return BadRequest("Файл изображения не передан или пустой.");
}

if (!await _context.Users.AsNoTracking().AnyAsync(x => x.UId == userId))
{
return BadRequest("Пользователь не найден");
}

string userPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/user/");

if (!Directory.Exists(userPath))
{
Directory.CreateDirectory(userPath);
}

var path = Path.Combine(userPath, $"{userId}{Path.GetExtension(file.File.FileName)}");
using (var stream = new FileStream(path, FileMode.Create))
{
await file.File.CopyToAsync(stream);
}

return Ok();
}
}
}
