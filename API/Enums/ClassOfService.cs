using NpgsqlTypes;

namespace API.Enums
{
    public enum ClassOfService
    {
        Эконом,
        Комфорт,
        Бизнес,
        [PgName("Первый класс")]
        Первый_класс
    }
}
