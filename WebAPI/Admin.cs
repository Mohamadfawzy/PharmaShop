using System;
using System.Collections.Generic;

namespace WebAPI;

public partial class Admin
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
