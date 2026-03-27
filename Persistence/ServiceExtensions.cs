using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.IdentityModels;
using Persistence.Seeds;
using Persistence.SharedServices;

namespace Persistence
{
    public static class ServiceExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            //Registering ApplicationDbContext as a services & Injecting ApplicationDbContext when needed
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DbConnStrg"));
            });

            services.AddDataProtection();

            //NOTE: MUST REGISTER AddIdentityCore, AddRoles, AddEntityFrameworkStores BEFORE,
            //we REGISTER services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            //Else we would get Registering service
            //is NOT FOUND
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>() //MUST ADDROLES FIRST
                .AddEntityFrameworkStores<ApplicationDbContext>() //THEN USE AddEntityFrameworkStores TO ADD ROLES TO db
                .AddDefaultTokenProviders();


            //after 3 FAILED logins, the App is lockout for 5 minuties 
            //and ONLY creating a new User is allow to access App
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;
            });

            //services.Configure<DataProtectionTokenProviderOptions>(...) in ASP.NET Core Identity
            //is used to customize the lifespan and behavior of security tokens,
            //such as those for password resets and email confirmation.
            //By setting TokenLifespan, developers can change the default 1-day expiration time,
            //commonly setting it to shorter durations like 15 minutes or a few hours for enhanced security.
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(30);
            });

            //Injecting ApplicationDbContext whenever IApplicationDbContext is requested
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.AddTransient<IAccountService, AccountService>();


            //Usage in Program.cs
            // This SeedRolesAsync & SeedUserAsync methods r typically called during
            // application startup(after var app = builder.Build();)
            // to ensure the roles exist when the application runs.
            // A dependency injection scope must be created to resolve the necessary services
            //these static classes' static methods seed roles & users

            //The services.BuildServiceProvider() method creates an instance of IServiceProvider
            //from a collection of registered services (IServiceCollection).
            //The IServiceProvider is the dependency injection (DI) container responsible
            //for resolving and managing service instances at runtime according to
            //their defined lifetimes (singleton, scoped, or transient).
            //
            //Enables Resolution: Once the service provider is built,
            //the application can request instances of services using methods like GetService<T>()
            //or through constructor injection.
            DefaultRoles.SeedRolesAsync(services.BuildServiceProvider()).Wait();
            DefaultUser.SeedUserAsync(services.BuildServiceProvider()).Wait();
            //In .NET, ServiceProvider is the default implementation of IServiceProvider,
            //acting as the container responsible for resolving dependency instances at runtime.
            //Built from an IServiceCollection, it provides methods like GetService<T>
            //and GetRequiredService<T> to manage service lifetimes and inject dependencies,
            //commonly used in ASP.NET Core applications for managing application services. 
        }
    }
}
