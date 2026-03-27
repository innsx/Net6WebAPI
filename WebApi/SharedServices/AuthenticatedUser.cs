using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApi.SharedServices
{
    public class AuthenticatedUser : IAuthenticatedUser
    {       
        public string UserId { get; set; }

        //Registration: Requires builder.Services.AddHttpContextAccessor(); in Program.cs.
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
