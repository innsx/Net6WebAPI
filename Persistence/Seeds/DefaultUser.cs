using Application.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Persistence.IdentityModels;

namespace Persistence.Seeds
{
    public static class DefaultUser
    {
        public async static Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            //UserManager<TUser>: This is a core service in ASP.NET Core Identity that provides APIs
            //for managing user data, including creating, deleting, updating, and finding users.

            //ApplicationUser: This is a custom class that typically inherits from IdentityUser
            //to add application-specific properties (e.g., first name, last name) to the default user model. 
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser();
            user.UserName = "superadmin";
            user.Email = "kou20.xiong21@gmail.com";
            user.EmailConfirmed = true;
            user.FirstName = "Nick";
            user.LastName = "Xiong";
            user.Gender = "Male";
            user.PhoneNumberConfirmed = true;

            if (userManager.Users.All(x => x.Id != user.Id))
            {
                var email = await userManager.FindByEmailAsync(user.Email);

                if (email is null)
                {
                    await userManager.CreateAsync(user, "123@Test");
                    await userManager.AddToRoleAsync(user, Roles.SuperAdmin.ToString());
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(user, Roles.Basic.ToString());
                }
            }
        }
    }
}
