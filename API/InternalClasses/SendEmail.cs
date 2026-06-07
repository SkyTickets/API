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
                Body = $"<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<style>\r\nbody {{\r\nfont-family: Arial, sans-serif;\r\nbackground-color: #f4f4f4;\r\nmargin: 0;\r\npadding: 0;\r\n}}\r\n.container {{\r\nwidth: 100%;\r\nmax-width: 600px;\r\nmargin: 0 auto;\r\nbackground-color: #ffffff;\r\nborder-radius: 8px;\r\noverflow: hidden;\r\nbox-shadow: 0 4px 10px rgba(0,0,0,0.1);\r\n}}\r\n.header {{\r\nbackground-color: #0056b3;\r\ncolor: #ffffff;\r\npadding: 20px;\r\ntext-align: center;\r\n}}\r\n.header h1 {{\r\nmargin: 0;\r\nfont-size: 24px;\r\n}}\r\n.content {{\r\npadding: 30px;\r\ncolor: #333333;\r\nline-height: 1.6;\r\n}}\r\n.footer {{\r\nbackground-color: #f9f9f9;\r\npadding: 20px;\r\ntext-align: center;\r\nfont-size: 12px;\r\ncolor: #777777;\r\n}}\r\n</style>\r\n</head>\r\n<body>\r\n<div class=\"container\">\r\n<div class=\"header\">\r\n<h1>Добро пожаловать в SkyTickets!</h1>\r\n</div>\r\n<div class=\"content\">\r\n<p>Здравствуйте!</p>\r\n<p>Спасибо за регистрацию в нашей системе бронирования авиабилетов.</p>\r\n<h2>Логин: {email}</h2><br/><h2>Пароль: {password}</h2>\r\n<p>Если вы не регистрировались на сайте SkyTickets, просто проигнорируйте это письмо.</p>\r\n</div>\r\n<div class=\"footer\">\r\n<p>&copy; 2026 SkyTickets. Все права защищены.<br>Уфа, Республика Башкортостан</p>\r\n</div>\r\n</div>\r\n</body>\r\n</html>",
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

        public static async Task SendReceiptAsync(IDbContextFactory<PostgresContext> contextFactory, int bookingId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var booking = await context.Bookings
                .Include(b => b.BUserNavigation)
                .Include(b => b.Tickets).ThenInclude(t => t.TPassengerNavigation)
                .Include(b => b.Tickets).ThenInclude(t => t.TsServices)
                .Include(b => b.BFlightNavigation).ThenInclude(f => f.FAirlineNavigation)
                .FirstOrDefaultAsync(b => b.BId == bookingId);

            if (booking is null || booking.BUserNavigation is null) return;

            var user = booking.BUserNavigation;
            var flight = booking.BFlightNavigation;

            string itemsRowsHtml = "";
            int positionNum = 1;

            foreach (var ticket in booking.Tickets)
            {
                string passengerName = $"{ticket.TPassengerNavigation.PSurname} {ticket.TPassengerNavigation.PName}";

                itemsRowsHtml += $@"
            <tr style=""border-bottom: 1px solid #f3f4f6;"">
                <td style=""padding: 10px 0; font-size: 13px; color: #374151;"">{positionNum++}. Авиабилет ({ticket.TClass}) — {passengerName}</td>
                <td style=""padding: 10px 0; font-size: 13px; color: #374151; text-align: right;"">{ticket.TPrice:N0} ₽</td>
            </tr>";

                foreach (var svc in ticket.TsServices)
                {
                    itemsRowsHtml += $@"
                <tr style=""border-bottom: 1px solid #f3f4f6;"">
                    <td style=""padding: 10px 0; font-size: 13px; color: #6b7280; padding-left: 12px;"">• Доп. услуга: {svc.AsName} ({passengerName})</td>
                    <td style=""padding: 10px 0; font-size: 13px; color: #374151; text-align: right;"">{svc.AsPrice:N0} ₽</td>
                </tr>";
                }
            }

            MailAddress from = new(_email, "SkyTickets Финансы");
            MailAddress to = new(user.UEmail);

            MailMessage mailMessage = new(from, to)
            {
                Subject = $"SkyTickets — Кассовый чек № CHK-{booking.BId}",
                IsBodyHtml = true,
                Body = $@"
<!doctype html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"" />
  <title>Электронный чек</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f3f4f6; font-family: Arial, sans-serif;"">
  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f3f4f6; padding: 30px 15px;"">
    <tr>
      <td align=""center"">
        <table width=""100%"" style=""max-width: 500px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); border-collapse: collapse;"">
          
          <!-- Заголовок чека -->
          <tr>
            <td style=""padding: 24px; text-align: center; border-bottom: 2px dashed #e5e7eb;"">
              <div style=""font-size: 22px; font-weight: 800; color: #1f2937; letter-spacing: 0.5px;"">SkyTickets</div>
              <div style=""font-size: 12px; color: #6b7280; margin-top: 4px;"">ООО «СКАЙ ТИКЕТС»</div>
              <div style=""font-size: 11px; color: #9ca3af; margin-top: 2px;"">ИНН 0274001234 · КПП 027401001</div>
            </td>
          </tr>

          <!-- Информация о транзакции -->
          <tr>
            <td style=""padding: 20px 24px 10px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 12px; color: #4b5563; line-height: 1.6;"">
                <tr>
                  <td>ЧЕК №: CHK-{booking.BId}-{DateTime.Now:yyyyMMdd}</td>
                  <td align=""right"">ДАТА: {DateTime.Now:dd.MM.yyyy HH:mm}</td>
                </tr>
                <tr>
                  <td>ТИП ОПЕРАЦИИ: ПРИХОД</td>
                  <td align=""right"">СИСТЕМА НАЛОГООБЛОЖЕНИЯ: ОСН</td>
                </tr>
                <tr>
                  <td colspan=""2"">ПОКУПАТЕЛЬ: {user.UEmail}</td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Список позиций -->
          <tr>
            <td style=""padding: 10px 24px;"">
              <div style=""border-top: 1px solid #e5e7eb; padding-top: 10px;"">
                <div style=""font-size: 11px; font-weight: 700; color: #9ca3af; margin-bottom: 8px;"">НАИМЕНОВАНИЕ ТОВАРОВ / УСЛУГ</div>
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                  {itemsRowsHtml}
                </table>
              </div>
            </td>
          </tr>

          <!-- Итоги -->
          <tr>
            <td style=""padding: 14px 24px 24px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""border-top: 2px solid #1f2937; padding-top: 14px;"">
                <tr style=""font-size: 18px; font-weight: 700; color: #1f2937;"">
                  <td>ИТОГ</td>
                  <td align=""right"">{booking.BTotalPrice:N0} ₽</td>
                </tr>
                <tr style=""font-size: 12px; color: #6b7280; margin-top: 4px;"">
                  <td>В том числе НДС 20%</td>
                  <td align=""right"">{(booking.BTotalPrice * 0.20):N2} ₽</td>
                </tr>
                <tr style=""font-size: 12px; color: #4b5563; padding-top: 8px;"">
                  <td>ФОРМА ОПЛАТЫ: БЕЗНАЛИЧНЫМИ</td>
                  <td align=""right"" style=""font-weight: 600;"">{booking.BTotalPrice:N0} ₽</td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Подвал чека -->
          <tr>
            <td style=""padding: 16px 24px; background-color: #f9fafb; text-align: center; border-radius: 0 0 8px 8px; font-size: 11px; color: #9ca3af; border-top: 1px solid #e5e7eb;"">
              Электронный чек доступен в ЛК авиакомпании.<br>
              Спасибо, что выбрали SkyTickets!
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>"
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

        public static async Task SendTicketAsync(IDbContextFactory<PostgresContext> contextFactory, int ticketid)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            Ticket? ticket = await context.Tickets
                .Include(t => t.TPassengerNavigation)         
                .Include(t => t.TsServices)                     
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                        .ThenInclude(f => f.FAirlineNavigation)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BUserNavigation)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                        .ThenInclude(f => f.FDepartureAirportNavigation)
                .Include(t => t.TBookingNavigation)
                    .ThenInclude(b => b.BFlightNavigation)
                        .ThenInclude(f => f.FArrivalAirportNavigation)
                .FirstOrDefaultAsync(t => t.TId == ticketid);

            if (ticket is null) return;

            var user = ticket.TBookingNavigation.BUserNavigation;
            var flight = ticket.TBookingNavigation.BFlightNavigation;
            var passenger = ticket.TPassengerNavigation;
            var airline = flight.FAirlineNavigation;
            var departureAirport = flight.FDepartureAirportNavigation;
            var arrivalAirport = flight.FArrivalAirportNavigation;

            string fullName = $"{passenger.PSurname} {passenger.PName} {passenger.PPatronymic}".Trim();
            string latinName = Transliteration.ToLatin(fullName);
            string stringClass = Convertation.ConvertEnumToString(ticket.TClass);

            var diff = flight.FArrivalTime - flight.FDepartureTime;
            string duration = $"{(int)diff.TotalHours}ч {diff.Minutes}м";

            string classColor = stringClass switch
            {
                "Комфорт" => "#4CAF50",
                "Бизнес" => "#FF9800",
                "Первый класс" => "#9C27B0",
                _ => "#2196F3"
            };

            string servicesHtml = "";
            if (ticket.TsServices.Count > 0)
            {
                int servicesTotal = ticket.TsServices.Sum(s => s.AsPrice);
                var rows = string.Join("", ticket.TsServices.Select(s =>
                    $"<tr>" +
                    $"<td style=\"padding: 6px 0; color: #555555; font-size: 13px;\">{s.AsName}</td>" +
                    $"<td style=\"padding: 6px 0; color: #555555; font-size: 13px; text-align: right;\">{s.AsPrice:N0} ₽</td>" +
                    $"</tr>"));

                servicesHtml = $@"
          <tr>
            <td style=""padding: 0 28px 24px;"">
              <div style=""background: #f8f7ff; border-radius: 10px; padding: 16px 20px;"">
                <div style=""font-size: 10px; font-weight: 700; color: #999999; letter-spacing: 0.5px; margin-bottom: 10px;"">ДОПОЛНИТЕЛЬНЫЕ УСЛУГИ</div>
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                  {rows}
                  <tr style=""border-top: 1px solid #e0dfff;"">
                    <td style=""padding-top: 8px; font-size: 13px; font-weight: 700; color: #1e1b4b;"">Итого за услуги</td>
                    <td style=""padding-top: 8px; font-size: 13px; font-weight: 700; color: #1e1b4b; text-align: right;"">{servicesTotal:N0} ₽</td>
                  </tr>
                </table>
              </div>
            </td>
          </tr>";
            }

            int totalPrice = ticket.TPrice + ticket.TsServices.Sum(s => s.AsPrice);

            MailAddress from = new(_email, "SkyTickets");
            MailAddress to = new(user.UEmail);

            MailMessage mailMessage = new(from, to)
            {
                Subject = $"SkyTickets — Электронный билет №{ticket.TId}",
                IsBodyHtml = true,
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
        <table width=""100%"" style=""max-width: 650px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 30px rgba(0,0,0,0.08); border-collapse: collapse;"">

          <!-- Шапка: авиакомпания + номер + статус -->
          <tr>
            <td style=""background: linear-gradient(135deg, #1565C0 0%, #1976D2 100%); padding: 24px 28px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td style=""color: #ffffff;"">
                    <div style=""font-size: 20px; font-weight: 600; letter-spacing: 0.5px;"">{airline?.AlName ?? "Авиакомпания"}</div>
                    <div style=""font-size: 13px; opacity: 0.75; margin-top: 4px;"">Электронный билет № {ticket.TId} · Бронирование № {ticket.TBooking}</div>
                  </td>
                  <td align=""right"">
                    <span style=""padding: 6px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; color: #ffffff; background-color: rgba(76,175,80,0.35); border: 1px solid rgba(255,255,255,0.25);"">
                      ✓ Куплен
                    </span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Маршрут -->
          <tr>
            <td style=""padding: 28px 28px 20px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td width=""38%"" valign=""top"">
                    <div style=""font-size: 40px; font-weight: 800; color: #0d0d0d; line-height: 1; font-variant-numeric: tabular-nums;"">{flight.FDepartureTime:HH:mm}</div>
                    <div style=""font-size: 15px; color: #1e1b4b; font-weight: 600; margin-top: 6px;"">{departureAirport?.ApName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 3px;"">{flight.FDepartureTime:d MMM yyyy}, {flight.FDepartureTime:ddd}</div>
                  </td>
                  <td width=""24%"" align=""center"" valign=""middle"" style=""padding: 0 8px;"">
                    <div style=""font-size: 12px; color: #888888; margin-bottom: 6px;"">{duration}</div>
                    <div style=""border-top: 2px solid #1976D2; position: relative; text-align: center;"">
                      <span style=""font-size: 18px; color: #1976D2; position: relative; top: -12px; background: #ffffff; padding: 0 8px;"">✈</span>
                    </div>
                    <div style=""font-size: 11px; color: #4CAF50; font-weight: 600; margin-top: 4px;"">Прямой рейс</div>
                  </td>
                  <td width=""38%"" align=""right"" valign=""top"">
                    <div style=""font-size: 40px; font-weight: 800; color: #0d0d0d; line-height: 1; font-variant-numeric: tabular-nums;"">{flight.FArrivalTime:HH:mm}</div>
                    <div style=""font-size: 15px; color: #1e1b4b; font-weight: 600; margin-top: 6px;"">{arrivalAirport?.ApName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 3px;"">{flight.FArrivalTime:d MMM yyyy}, {flight.FArrivalTime:ddd}</div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Пунктирная линия отрыва -->
          <tr>
            <td style=""padding: 0;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td width=""16"" height=""28"" style=""background-color: #f0f4f8; border-radius: 0 14px 14px 0;""></td>
                  <td style=""border-bottom: 2px dashed #d1d5db;"">&nbsp;</td>
                  <td width=""16"" height=""28"" style=""background-color: #f0f4f8; border-radius: 14px 0 0 14px;""></td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Детали: пассажир / класс / дата / стоимость билета -->
          <tr>
            <td style=""padding: 24px 28px 20px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""table-layout: fixed;"">
                <tr>
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-right: 12px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #aaaaaa; letter-spacing: 0.8px; margin-bottom: 6px;"">ПАССАЖИР</div>
                    <div style=""font-size: 14px; font-weight: 600; color: #1a1a1a; word-wrap: break-word;"">{fullName}</div>
                    <div style=""font-size: 12px; color: #888888; margin-top: 3px; font-style: italic;"">{latinName}</div>
                  </td>
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-left: 14px; padding-right: 12px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #aaaaaa; letter-spacing: 0.8px; margin-bottom: 6px;"">КЛАСС</div>
                    <span style=""display: inline-block; color: #ffffff; background-color: {classColor}; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 700;"">{stringClass}</span>
                  </td>
                  <td valign=""top"" style=""border-right: 1px solid #f0f0f0; padding-left: 14px; padding-right: 12px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #aaaaaa; letter-spacing: 0.8px; margin-bottom: 6px;"">ДАТА ПОКУПКИ</div>
                    <div style=""font-size: 14px; font-weight: 600; color: #1a1a1a;"">{ticket.TBookingNavigation.BCreatedAt:d MMM yyyy}</div>
                  </td>
                  <td valign=""top"" align=""right"" style=""padding-left: 14px;"">
                    <div style=""font-size: 10px; font-weight: 700; color: #aaaaaa; letter-spacing: 0.8px; margin-bottom: 6px;"">ТАРИФ</div>
                    <div style=""font-size: 16px; font-weight: 700; color: #1565C0;"">{ticket.TPrice:N0} ₽</div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Доп. услуги (если есть) -->
          {servicesHtml}

          <!-- Итоговая стоимость -->
          <tr>
            <td style=""padding: 0 28px 28px;"">
              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td style=""background: #1e1b4b; border-radius: 10px; padding: 14px 20px;"">
                    <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                      <tr>
                        <td style=""color: #a5b4fc; font-size: 13px;"">Итого к оплате</td>
                        <td align=""right"" style=""color: #ffffff; font-size: 22px; font-weight: 800;"">{totalPrice:N0} ₽</td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>

        <div style=""text-align: center; color: #aaaaaa; font-size: 12px; margin-top: 18px; max-width: 650px;"">
          Для посадки на борт предъявите этот билет и документ, удостоверяющий личность.<br>
          © 2026 SkyTickets. Уфа, Республика Башкортостан
        </div>
      </td>
    </tr>
  </table>
</body>
</html>"
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