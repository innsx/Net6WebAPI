using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.IdentityModels;

namespace Persistence.Context
{
    //ApplicationDbContext class inherits from the generic IdentityDbContext class provided by the
    //Microsoft.AspNetCore.Identity.EntityFrameworkCore library
    // which is a Base class for the Entity Framework database context used for identity.

    //ApplicationUser: Specifies a custom user entity class.
    //This class must, in turn, inherit from IdentityUser<Guid> to use GUIDs as primary keys.

    //ApplicationRole: Specifies a custom role entity class.
    //This class must, in turn, inherit from IdentityRole<Guid>.

    //Guid: Specifies that the primary keys for the User, Role, UserClaim, RoleClaim, UserLogin,
    //and UserToken tables should be of type Guid instead of the default string.

    //IApplicationDbContext: This indicates that the class also implements a custom interface named IApplicationDbContext.
    //This pattern is commonly used to enable dependency injection and
    //facilitate unit testing within a clean architecture design
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        //ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        //is a constructor for an Entity Framework (EF) Core DbContext class,
        //specifically designed to enable dependency injection (DI) in ASP.NET Core applications.

        //DbContextOptions<ApplicationDbContext> options:
        //This parameter accepts an options object that contains configuration information,
        //such as the database connection string and the specific database provider
        //(e.g., SQL Server, SQLite) to be used.

        //: base(options): This is C# syntax for a constructor initializer.
        //It calls the constructor of the base class (DbContext) and passes the options parameter to it.
        //This ensures the base DbContext is correctly configured with the application's settings
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product>? Products1 { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}
