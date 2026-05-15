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
                Body = $"<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <style>\r\n        body {{\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f4f4f4;\r\n            margin: 0;\r\n            padding: 0;\r\n        }}\r\n        .container {{\r\n            width: 100%;\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: #ffffff;\r\n            border-radius: 8px;\r\n            overflow: hidden;\r\n            box-shadow: 0 4px 10px rgba(0,0,0,0.1);\r\n        }}\r\n        .header {{\r\n            background-color: #0056b3;\r\n            color: #ffffff;\r\n            padding: 20px;\r\n            text-align: center;\r\n        }}\r\n        .header h1 {{\r\n            margin: 0;\r\n            font-size: 24px;\r\n        }}\r\n        .content {{\r\n            padding: 30px;\r\n            color: #333333;\r\n            line-height: 1.6;\r\n        }}\r\n        .verification-code {{\r\n            display: block;\r\n            width: fit-content;\r\n            margin: 20px auto;\r\n            padding: 15px 30px;\r\n            background-color: #e7f3ff;\r\n            border: 2px dashed #0056b3;\r\n            font-size: 32px;\r\n            font-weight: bold;\r\n            color: #0056b3;\r\n            letter-spacing: 5px;\r\n        }}\r\n        .footer {{\r\n            background-color: #f9f9f9;\r\n            padding: 20px;\r\n            text-align: center;\r\n            font-size: 12px;\r\n            color: #777777;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <h1>Добро пожаловать в SkyTickets!</h1>\r\n        </div>\r\n        <div class=\"content\">\r\n            <p>Здравствуйте!</p>\r\n            <p>Спасибо за регистрацию в нашей системе бронирования авиабилетов. </p>\r\n            \r\n            <h2>Логин: {email}</h2><br/><h2>Пароль: {password}</h2>\r\n            \r\n            <p>Если вы не регистрировались на сайте SkyTickets, просто проигнорируйте это письмо.</p>\r\n        </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2026 SkyTickets. Все права защищены.<br>Уфа, Республика Башкортостан</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>",
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
            Ticket ticket = await context.Tickets.FirstAsync(t => t.TId == ticketid);
            User user = await context.Users.FirstAsync(t => t.UId == ticket.TUser);
            Flight flight = await context.Flights.FirstAsync(t => t.FId == ticket.TFlight);
            Airline airline = await context.Airlines.FirstAsync(t => t.AlId == flight.FAirline);
            Airport arrivalairport = await context.Airports.FirstAsync(t => t.ApId == flight.FArrivalAirport);
            Airport departureairport = await context.Airports.FirstAsync(t => t.ApId == flight.FDepartureAirport);
            MailAddress from = new(_email, "SkyTickets");
            MailAddress to = new(user.UEmail);
 
            MailMessage mailMessage = new(from, to)
            {
                Subject = $"Билет №{ticket.TId}",
                Body = $"<!doctype html>\r\n<html lang=\"ru\">\r\n  <head>\r\n    <meta charset=\"UTF-8\" />\r\n    <title>Авиабилет</title>\r\n    <style>\r\n      body {{\r\n        font-family: Arial, sans-serif;\r\n        background-color: #ffffff;\r\n        color: #000000;\r\n        margin: 0;\r\n        padding: 0;\r\n      }}\r\n      .header {{\r\n        background-color: #f0f0f0;\r\n        color: #000000;\r\n        padding: 10px 20px;\r\n        text-align: center;\r\n      }}\r\n      .ticket-info {{\r\n        padding: 20px;\r\n        max-width: 900px;\r\n        margin: 0 auto;\r\n      }}\r\n      .ticket-section {{\r\n        background-color: #ffffff;\r\n        border: 1px solid #cccccc;\r\n        padding: 15px;\r\n        margin-bottom: 20px;\r\n        border-radius: 5px;\r\n      }}\r\n      .flight-title {{\r\n        font-size: 18px;\r\n        font-weight: bold;\r\n        text-align: center;\r\n        margin-bottom: 10px;\r\n      }}\r\n      .flight-detail {{\r\n        display: flex;\r\n        justify-content: space-between;\r\n        padding: 5px 0;\r\n        border-bottom: 1px solid #cccccc;\r\n        width: 100%;\r\n      }}\r\n      .flight-detail:last-child {{\r\n        border-bottom: none;\r\n      }}\r\n      .flight-detail span {{\r\n        flex: 1;\r\n        text-align: center;\r\n        word-wrap: break-word;\r\n        padding: 0 5px;\r\n      }}\r\n    </style>\r\n  </head>\r\n  <body>\r\n    <div class=\"header\">\r\n      <h1>SkyTickets</h1>\r\n      <p>Электронный билет</p>\r\n    </div>\r\n    <div class=\"ticket-info\">\r\n      <div class=\"ticket-section\">\r\n        <p>Номер заказа: {ticket.TId}</p>\r\n        <p>Класс обслуживания: {Convertation.ConvertEnumToString(ticket.TClass)}</p>\r\n        <p>Электронный билет:  {ticket.TId}</p>\r\n      </div>\r\n      <div class=\"ticket-section\">\r\n        <p>Пассажир: {user.USurname} {user.UName} {user.UPatronymic}</p>\r\n      </div>\r\n      <div class=\"flight-title\">{departureairport.ApCountry} {departureairport.ApCity} — {arrivalairport.ApCountry} {arrivalairport.ApCity}</div>\r\n      <div class=\"ticket-section\">\r\n        <div class=\"flight-detail\">\r\n          <span>{airline.AlName}</span>\r\n         </div>\r\n        <div class=\"flight-detail\">\r\n          <span>{departureairport.ApName}</span>\r\n          <span>{flight.FDepartureTime:dd.MM.yyyy, HH:mm}</span>\r\n          <span> - </span>\r\n          <span>{arrivalairport.ApName}</span>\r\n          <span>{flight.FArrivalTime:dd.MM.yyyy, HH:mm}</span>\r\n        </div>\r\n      </div>\r\n      <div class=\"ticket-section\">\r\n        <p>Номер билета для регистрации: {ticket.TId}</p>\r\n        <p>Всего в пути: {(flight.FArrivalTime - flight.FDepartureTime).Hours} часов {(flight.FArrivalTime - flight.FDepartureTime).Minutes} минут</p>\r\n        <p>Дата покупки билета: {ticket.TBoughtDate:dd.MM.yyyy}</p>\r\n        <p>Стоимость: {ticket.TTotalPrice} ₽</p>\r\n      </div>\r\n    </div>\r\n  </body>\r\n</html>\r\n",
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

        private const string _email = "artembruh321@yandex.ru";
        private const string _appPassword = "rrmrdyhgytvaxtez";

    }
}
