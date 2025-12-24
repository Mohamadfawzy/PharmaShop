using Microsoft.AspNetCore.Identity;

namespace Repository.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public string? NameEn { get; set; }
    public string? Description { get; set; }
}