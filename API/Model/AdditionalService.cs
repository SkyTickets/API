using API.ExportClasses;
using System;
using System.Collections.Generic;

namespace API.Model;

public partial class AdditionalService
{
    public int AsId { get; set; }

    public string AsName { get; set; } = null!;

    public int AsPrice { get; set; }

    public virtual ICollection<Ticket> TsTickets { get; set; } = new List<Ticket>();

    public ExportAdditionalService ToExport()
    {
        return new()
        {
            AsId = AsId,
            AsName = AsName,
            AsPrice = AsPrice,
        };
    }
}
