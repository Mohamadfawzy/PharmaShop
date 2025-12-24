using Microsoft.AspNetCore.Identity;

namespace Repository.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public bool IsActive { get; set; } = true;

}
