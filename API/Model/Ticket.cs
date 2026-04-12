using API.Enums;
using API.ExportClasses;
using API.InternalClasses;

namespace API.Model;

public partial class Ticket
{
    public int TId { get; set; }

    public int TFlight { get; set; }

    public int TUser { get; set; }

    public DateTime TBoughtDate { get; set; }

    public ClassOfService TClass { get; set; }

    public int TTotalPrice { get; set; }

    public TicketStatus TStatus { get; set; }

    public virtual Flight TFlightNavigation { get; set; } = null!;

    public virtual User TUserNavigation { get; set; } = null!;

    public ExportTicket ToExport()
    {
        return new()
        {
            TId = TId,
            TFlight = TFlight,
            TUser = $"{TUserNavigation.USurname} {TUserNavigation.UName} {TUserNavigation.UPatronymic}",
            TBoughtDate = TBoughtDate,
            TClass = Convertation.ConvertEnumToString(TClass),
            TTotalPrice = TTotalPrice,
            TStatus = Convertation.ConvertEnumToString(TStatus),
        };
    }
}
