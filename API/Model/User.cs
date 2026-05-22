using API.Enums;
using API.ExportClasses;
using API.InternalClasses;
using System;
using System.Collections.Generic;

namespace API.Model;

public partial class User
{
    public int UId { get; set; }

    public string USurname { get; set; } = null!;

    public string UName { get; set; } = null!;

    public string? UPatronymic { get; set; }

    public string UEmail { get; set; } = null!;

    public string UPassword { get; set; } = null!;

    public Role URole { get; set; }

    public string UPhone { get; set; } = null!;

    public DateOnly UBirthdate { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    internal ExportUser ToExport()
    {
        return new()
        {
            UId = UId,
            USurname = USurname,
            UName = UName,
            UEmail = UEmail,
            UPassword = UPassword,
            UPhone = UPhone,
            UBirthdate = UBirthdate,
            UPatronymic = UPatronymic,
            URole = Convertation.ConvertEnumToString(URole),
        };
    }

    public int GetUserId(string email)
    {
        return email == UEmail ? UId : -1;
    }
}
