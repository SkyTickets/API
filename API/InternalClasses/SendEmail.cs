using API.Model;

using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace API.InternalClasses
{
    internal static class SendEmail
    {
        public static async Task SendLoginInformationAsync(string email, string password)
        {
            MailAddress from = new(_email, "SkyTickets");
            MailAddress to = new(email);

            MailMessage mailMessage = new(from, to)
            {
                Subject = "Информация для входа в приложение",
                Body = $"<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<style>\r\nbody {{\r\nfont-family: Arial, sans-serif;\r\nbackground-color: #f4f4f4;\r\nmargin: 0;\r\npadding: 0;\r\n}}\r\n.container {{\r\nwidth: 100%;\r\nmax-width: 600px;\r\nmargin: 0 auto;\r\nbackground-color: #ffffff;\r\nborder-radius: 8px;\r\noverflow: hidden;\r\nbox-shadow: 0 4px 10px rgba(0,0,0,0.1);\r\n}}\r\n.header {{\r\nbackground-color: #0056b3;\r\ncolor: #ffffff;\r\npadding: 20px;\r\ntext-align: center;\r\n}}\r\n.header h1 {{\r\nmargin: 0;\r\nfont-size: 24px;\r\n}}\r\n.content {{\r\npadding: 30px;\r\ncolor: #333333;\r\nline-height: 1.6;\r\n}}\r\n.verification-code {{\r\ndisplay: block;\r\nwidth: fit-content;\r\nmargin: 20px auto;\r\npadding: 15px 30px;\r\nbackground-color: #e7f3ff;\r\nborder: 2px dashed #0056b3;\r\nfont-size: 32px;\r\nfont-weight: bold;\r\ncolor: #0056b3;\r\nletter-spacing: 5px;\r\n}}\r\n.footer {{\r\nbackground-color: #f9f9f9;\r\npadding: 20px;\r\ntext-align: center;\r\nfont-size: 12px;\r\ncolor: #777777;\r\n}}\r\n</style>\r\n</head>\r\n<body>\r\n<div class=\"container\">\r\n<div class=\"header\">\r\n<h1>Добро пожаловать в SkyTickets!</h1>\r\n</div>\r\n<div class=\"content\">\r\n<p>Здравствуйте!</p>\r\n<p>Спасибо за регистрацию в нашей системе бронирования авиабилетов. </p>\r\n<h2>Логин: {email}</h2><br/><h2>Пароль: {password}</h2>\r\n\r\n<p>Если вы не регистрировались на сайте SkyTickets, просто проигнорируйте это письмо.</p>\r\n</div>\r\n<div class=\"footer\">\r\n<p>&copy; 2026 SkyTickets. Все права защищены.<br>Уфа, Республика Башкортостан</p>\r\n</div>\r\n</div>\r\n</body>\r\n</html>",
                IsBodyHtml = true
            };

            SmtpClient smtp = new("smtp.yandex.com", 587)
            {
                Credentials = new NetworkCredential(_email, _appPassword.Replace(" ", "")),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            await smtp.SendMailAsync(mailMessage);
        }

        public static async Task SendTicketAsync(PostgresContext context, int ticketid)
        {
            // Загружаем билет со всеми связями сразу через Include, чтобы избежать кучи ручных запросов к контексту
            Ticket? ticket = await context.Tickets
                .Include(t => t.TUserNavigation)
                .Include(t => t.TFlightNavigation)
                    .ThenInclude(f => f.FAirlineNavigation)
                .Include(t => t.TFlightNavigation)
                    .ThenInclude(f => f.FDepartureAirportNavigation)
                .Include(t => t.TFlightNavigation)
                    .ThenInclude(f => f.FArrivalAirportNavigation)
                .FirstOrDefaultAsync(t => t.TId == ticketid);

            if (ticket is null) return;

            var flight = ticket.TFlightNavigation;
            var user = ticket.TUserNavigation;
            var airline = flight.FAirlineNavigation;
            var departureAirport = flight.FDepartureAirportNavigation;
            var arrivalAirport = flight.FArrivalAirportNavigation;

            // Данные для заполнения шаблона
            string fullName = $"{user.USurname} {user.UName} {user.UPatronymic}";
            string latinName = Transliteration.ToLatin(fullName);
            string stringClass = Convertation.ConvertEnumToString(ticket.TClass);

            // Расчет времени в пути
            var diff = flight.FArrivalTime - flight.FDepartureTime;
            string duration = $"{(int)diff.TotalHours}ч {diff.Minutes}м";

            // Определение цвета для плашки класса
            string classColor = stringClass switch
            {
                "Комфорт" => "#4CAF50",
                "Бизнес" => "#FF9800",
                "Первый класс" => "#9C27B0",
                _ => "#2196F3" // Эконом
            };

            MailAddress from = new(_email, "SkyTickets");
            MailAddress to = new(user.UEmail);

            MailMessage mailMessage = new(from, to)
            {
                Subject = $"SkyTickets — Электронный билет №{ticket.TId}",
                Body = $@"
<!doctype html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"" />
  <title>Электронный билет</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f0f4f8; font-family: 'Segoe UI', Helvetica, Arial, sans-serif;"">
  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f0f4f8; padding: 30px 15px;"">
    <tr>
      <td align=""center"">
        <table width=""100%"" id=""ticket-body"" style=""max-width: 650px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 30px rgba(0,0,0,0.08); border-collapse: collapse;"">
          
          <tr>
            <td style=""background: #1565C0; background: linear-gradient(135deg, #1565C0 0%, #1976D2 100%); padding: 24px 28px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td style=""color: #ffffff;"">
                    <div style=""font-size: 20px; font-weight: 600; letter-spacing: 0.5px;"">{airline?.AlName ?? "Авиакомпания"}</div>
                    <div style=""font-size: 13px; opacity: 0.8; margin-top: 4px;"">Билет № {ticket.TId}</div>
                  </td>
                  <td align=""right"">
                    <span style=""padding: 6px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; color: #ffffff; background-color: rgba(76, 175, 80, 0.3); border: 1px solid rgba(255,255,255,0.2);"">
                      Куплен
                    </span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td style=""padding: 28px 28px 24px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td width=""35%"" valign=""top"">
                    <div style=""font-size: 36px; font-weight: 700; color: #0d0d0d; line-height: 1;"">{flight.FDepartureTime:HH:mm}</div>
                    <div style=""font-size: 15px; color: #333333; font-weight: 500; margin-top: 6px;"">{departureAirport?.ApName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 3px;"">{flight.FDepartureTime:d MMM yyyy}, {flight.FDepartureTime:ddd}</div>
                  </td>
                  
                  <td width=""30%"" align=""center"" valign=""middle"" style=""padding: 0 10px;"">
                    <div style=""font-size: 13px; color: #666666; margin-bottom: 4px;"">{duration}</div>
                    <div style=""position: relative; border-top: 2px solid #1976D2; margin: 5px 0; text-align: center;"">
                      <span style=""font-size: 16px; color: #1976D2; position: relative; top: -11px; background: #ffffff; padding: 0 6px;"">✈</span>
                    </div>
                    <div style=""font-size: 12px; color: #4CAF50; font-weight: 500; margin-top: 4px;"">Прямой рейс</div>
                  </td>
                  
                  <td width=""35%"" align=""right"" valign=""top"">
                    <div style=""font-size: 36px; font-weight: 700; color: #0d0d0d; line-height: 1;"">{flight.FArrivalTime:HH:mm}</div>
                    <div style=""font-size: 15px; color: #333333; font-weight: 500; margin-top: 6px;"">{arrivalAirport?.ApName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 3px;"">{flight.FArrivalTime:d MMM yyyy}, {flight.FArrivalTime:ddd}</div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td style=""padding: 0;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td width=""12"" height=""24"" style=""background-color: #f0f4f8; border-radius: 0 12px 12px 0;""></td>
                  <td style=""border-bottom: 2px dashed #dddddd;"">&nbsp;</td>
                  <td width=""12"" height=""24"" style=""background-color: #f0f4f8; border-radius: 12px 0 0 12px;""></td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td style=""padding: 24px 28px 28px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""table-layout: fixed;"">
                <tr>
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-right: 10px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #999999; letter-spacing: 0.5px; margin-bottom: 6px;"">ПАССАЖИР</div>
                    <div style=""font-size: 14px; font-weight: 600; color: #1a1a1a; word-wrap: break-word;"">{fullName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 2px; font-style: italic;"">{latinName}</div>
                  </td>
                  
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-left: 15px; padding-right: 10px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #999999; letter-spacing: 0.5px; margin-bottom: 6px;"">КЛАСС</div>
                    <span style=""display: inline-block; color: #ffffff; background-color: {classColor}; padding: 3px 10px; border-radius: 12px; font-size: 12px; font-weight: 700;"">
                      {stringClass}
                    </span>
                  </td>
                  
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-left: 15px; padding-right: 10px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #999999; letter-spacing: 0.5px; margin-bottom: 6px;"">ДАТА ПОКУПКИ</div>
                    <div style=""font-size: 14px; font-weight: 600; color: #1a1a1a;"">{ticket.TBoughtDate:d MMM yyyy}</div>
                  </td>
                  
                  <td valign=""top"" align=""right"" style=""padding-left: 15px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #999999; letter-spacing: 0.5px; margin-bottom: 6px;"">СТОИМОСТЬ</div>
                    <div style=""font-size: 18px; font-weight: 700; color: #1565C0;"">{ticket.TTotalPrice:N0} ₽</div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>

        <div style=""text-align: center; color: #999999; font-size: 13px; margin-top: 20px; max-width: 650px;"">
          Для посадки на борт предъявите этот билет и документ, удостоверяющий личность.
        </div>
      </td>
    </tr>
  </table>
</body>
</html>",
                IsBodyHtml = true
            };

            using SmtpClient smtp = new("smtp.yandex.com", 587)
            {
                Credentials = new NetworkCredential(_email, _appPassword.Replace(" ", "")),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            await smtp.SendMailAsync(mailMessage);
        }

        private const string _email = "artembruh321@yandex.ru";
        private const string _appPassword = "rrmrdyhgytvaxtez";

    }
}
