using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Pharmacist
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string FullNameEn { get; set; } = null!;

    public string? Specialty { get; set; }

    public int? UserId { get; set; }

    public virtual AspNetUser? User { get; set; }
}
