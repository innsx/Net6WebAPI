
using Application.FluentValidations;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class ServiceExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            //Assembly Scanning: The Assembly.GetExecutingAssembly() argument tells AutoMapper
            //to scan the specific assembly where this line of code is running
            //      (typically the main application project,
            //like an API or MVC project) for classes that inherit from Profile.

            //Automatic Profile Discovery: Any classes inheriting from Profile that are found in that assembly
            //will be automatically registered, eliminating the need to manually list each mapping configuration.
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            //Key Considerations with Multi-Project Solutions:
            //If your mapping profiles are located in a different project (assembly),
            //Assembly.GetExecutingAssembly() will not find them.
            //In such cases, you must specify the assembly containing the profiles,
            //for example, by using a marker type from that assembly:
            //    services.AddAutoMapper(typeof(UserProfileMarker).Assembly);.

            //Scanning All Assemblies:
            //To scan all loaded assemblies in the application domain (useful for solutions with many class libraries),
            //you can use      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());,
            //although this can include unwanted .NET framework assemblies, so filtering is recommended         


            //conf => conf.RegisterServicesFromAssembly(...):
            //This configuration action tells MediatR which assemblies to scan for types
            //that implement MediatR interfaces (like IRequestHandler, IRequest, INotificationHandler, etc.).
            services.AddMediatR(conf => conf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            ////Assembly Scanning: The Assembly.GetExecutingAssembly() argument tells FluentValidation
            //to scan the specific assembly where this line of code is running
            //      (typically the main application project and then Register Fluent Validations
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

           //This code registers a custom FluentValidationBehaviors<,> class as a generic pipeline behavior
           //for MediatR in ASP.NET Core.
           //It acts as middleware, automatically validating incoming commands or queries using FluentValidation
           //before they reach their handler.
           //It is added in Program.cs or Startup.cs to ensure all requests are validated
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviors<,>));
            //Purpose: Ensures every request validated via FluentValidation automatically before processing.

            //Pipeline Behavior: Implements IPipelineBehavior<TRequest, TResponse>, allowing pre/post-processing of requests.

            //Dependency Injection: Uses AddTransient to create a new instance of the validator behavior for each request.

            //Typical Workflow: The validator catches invalid requests and throws a ValidationException, preventing the handler from executing.
        }
    }
}
