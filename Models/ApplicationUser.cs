using Microsoft.AspNetCore.Identity;
namespace property_lease_saas.Models;
public class ApplicationUser: IdentityUser
{
    public string FullName {set;get;}
    public string UserType {set;get;}
}