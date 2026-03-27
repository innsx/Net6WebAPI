using Microsoft.AspNetCore.Identity;

namespace Persistence.IdentityModels
{

    //By inheriting from IdentityUser<Guid>, you create a custom ApplicationUser class
    //that can EXTEND or INCLUDE additional properties (e.g., FirstName, LastName, DateCreated, etc...)
    //beyond the default ones provided by IdentityUser
    //(like UserName, Email, PasswordHash, SecurityStamp, PhoneNumber, etc.
    public class ApplicationUser : IdentityUser<Guid>
    {
        //additional Properties added to IdentityUser built-in
        //(like UserName, Email, PasswordHash, SecurityStamp, PhoneNumber, etc.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }

}
