using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApi.SharedServices
{
    public class AuthenticatedUser : IAuthenticatedUser
    {       
        public string UserId { get; set; } = string.Empty;

        //The code public AuthenticatedUser(IHttpContextAccessor httpContextAccessor)
        //  is a constructor for a custom service in an ASP.NET Core application,
        //  typically used to access the current authenticated user's information from
        //  a non-controller class via dependency injection. 

        //IHttpContextAccessor: An interface provided by ASP.NET Core that allows access to the current HttpContext
        //  from outside of controllers or middleware components
        public AuthenticatedUser(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor is not null)
            {
                //UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); //returns superAdmin

                //.HttpContext: This property gets the context object for the current HTTP request.

                //! (null-forgiveness operator): This C# operator tells the compiler to suppress
                //the warning that the HttpContext might be null,
                //asserting that it will not be null at runtime in this context.
                UserId = httpContextAccessor.HttpContext!.User.FindFirstValue("uid");

                if (UserId is null)
                {
                    throw new NullReferenceException(nameof(UserId));
                }
            }
        }

    }
}
