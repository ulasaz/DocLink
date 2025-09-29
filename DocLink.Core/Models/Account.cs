using Microsoft.AspNetCore.Identity;

namespace DocLink.Core.Models;

public class Account : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? ImageUrl { get; set; }
}