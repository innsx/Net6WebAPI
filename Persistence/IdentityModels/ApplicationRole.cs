using Microsoft.AspNetCore.Identity;

namespace Persistence.IdentityModels
{
    //defines a custom role class for ASP.NET Core Identity
    //where the primary key (Id) type is explicitly set to Guid instead of the default string.

    //Primary Key Type: By default, IdentityRole uses string for its primary key.
    //Specifying <Guid> changes this type, which requires corresponding changes in other parts of the Identity setup.

    //Database Context Configuration: The ApplicationDbContext must be updated to use this custom class
    //and the Guid key type.
    //It would typically inherit from IdentityDbContext<TUser, TRole, TKey>,
    //such as IdentityDbContext<ApplicationUser, ApplicationRole, Guid>.

    //Service Configuration: In the application startup (e.g., Program.cs),
    //you must configure the Identity services to use your custom role class.
    //This often involves a line like:
    //  builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    //  .AddEntityFrameworkStores<ApplicationDbContext>();

    //You can EXTEND or INCLUDE additional properties beside the BUILT_IN properties i.e. 
    //Name, NormalizeName, ConcurrencyStamp
    public class ApplicationRole : IdentityRole<Guid>
    {

    }
}
