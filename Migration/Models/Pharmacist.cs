using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class Pharmacist
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Specialty { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
