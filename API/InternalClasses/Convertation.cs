using API.Enums;

namespace API.InternalClasses
{
    internal static class Convertation
    {
        public static string ConvertEnumToString(Role role)
        {
            return role switch
            {
                Role.Менеджер => "Менеджер",
                Role.Клиент => "Клиент",
                _ => "Неизвестно"
            };
        }

        public static string ConvertEnumToString(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Куплен => "Куплен",
                TicketStatus.Отменен => "Отменен",
                _ => "Неизвестно"
            };
        }

        public static string ConvertEnumToString(ClassOfService service)
        {
            return service switch
            {
                ClassOfService.Эконом => "Эконом",
                ClassOfService.Комфорт => "Комфорт",
                ClassOfService.Бизнес => "Бизнес",
                ClassOfService.Первый_класс => "Первый класс",
                _ => "Неизвестно"
            };
        }

        public static Enum? ConvertStringToEnum<T>(string value) where T : Enum
        {
            T? type = default;
            if (type is Role)
            {
                return value switch
                {
                    "Клиент" => Role.Клиент,
                    "Менеджер" => Role.Менеджер,
                    _ => Role.Клиент
                };
            }

            if (type is TicketStatus)
            {
                return value switch
                {
                    "Куплен" => TicketStatus.Куплен,
                    "Отменен" => TicketStatus.Отменен,
                    _ => TicketStatus.Куплен
                };
            }

            if (type is ClassOfService)
            {
                return value switch
                {
                    "Бизнес" => ClassOfService.Бизнес,
                    "Эконом" => ClassOfService.Эконом,
                    "Первый класс" => ClassOfService.Первый_класс,
                    "Комфорт" => ClassOfService.Комфорт,
                    _ => ClassOfService.Эконом
                };
            }
            return type;
        }
    }
}
