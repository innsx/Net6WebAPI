using Application.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Persistence.IdentityModels;

namespace Persistence.Seeds
{
    public static class DefaultRoles
    {
        //This SeedRolesAsync method a common pattern in ASP.NET Core applications to programmatically
        //seed default user roles into the database using the Dependency Injection container

        //IServiceProvider: This interface is the central component of ASP.NET Core's DI system used to resolve services. The serviceProvider instance gives access to all registered services.
        public async static Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            //The code serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            //is used to retrieve the RoleManager service from the dependency injection (DI) container.

            //This is typically done when manually resolving services outside
            //of the normal constructor injection pipeline,
            //such as during application startup for database seeding.

            //GetRequiredService<T>(): This extension method retrieves the service of the
            //specified type T (RoleManager<ApplicationRole> in this case).
            //If the service has not been registered in the DI container, it will throw an InvalidOperationException.

            //RoleManager<ApplicationRole>: This is an ASP.NET Core Identity class that provides the API
            //for managing roles in a persistence store.
            //ApplicationRole is your custom class representing a role,
            //which should inherit from IdentityRole
            var managerRole = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var superAdminRole = new ApplicationRole();
            superAdminRole.Name = Roles.SuperAdmin.ToString();
            superAdminRole.NormalizedName = Roles.SuperAdmin.ToString().ToUpper();
            await managerRole.CreateAsync(superAdminRole);


            var adminRole = new ApplicationRole();
            adminRole.Name = Roles.Admin.ToString();
            adminRole.NormalizedName = Roles.Admin.ToString().ToUpper();
            await managerRole.CreateAsync(adminRole);


            var basicRole = new ApplicationRole();
            basicRole.Name = Roles.Basic.ToString().ToUpper();
            basicRole.NormalizedName = Roles.Basic.ToString().ToUpper();
            await managerRole.CreateAsync(basicRole);
        }
    }
}
