using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApi.Services
{
    public static class AuthenticationServiceExt
    {
        public static void AddAuthenticationServiceExt(this IServiceCollection services, IConfiguration configuration)
        {
            //Registering Authentication with JWT Bear scheme
            services.AddAuthentication(options =>
            {
                /*
                 Default Behavior: When a request is made to an endpoint marked with the [Authorize] attribute 
                    (without specifying a scheme), the application will use the JwtBearer handler to validate the user's credentials.
                Authentication Process: The JwtBearer handler expects a JWT token, 
                    typically sent in the HTTP Authorization header in the format Bearer <token>.
                Scheme Name: JwtBearerDefaults.AuthenticationScheme is a constant string with the value "Bearer". 
                    Using this constant ensures consistency and helps avoid typos.
                Context: This setting is crucial in API-only projects where cookie authentication 
                    (the typical default in web apps) is not used, allowing the API to rely solely on stateless JWTs for security.     
                 */

                //DefaultAuthenticateScheme: The scheme used to authenticate a request (building the user's identity/principal)
                //sets the default authentication mechanism for your ASP.NET Core application
                //to use JWT (JSON Web Token) bearer tokens when processing incoming requests

                // the Middleware will look for JWT Token in an INCOMING request by DEFAULT
                //JwtBearerDefaults.AuthenticationScheme tells how the Application will try to Authenticate
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                //DefaultChallengeScheme: The scheme used when an unauthenticated user tries to access
                //a protected resource (e.g., returning a 401 Unauthorized response).

                //this is how the Application will CHALLENGE UNAUTHORIZED requests
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                //OPTION: DefaultScheme: A fallback default for all authentication actions.
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.SaveToken = false;

                //new TokenValidationParameters() is a class in .NET (Microsoft.IdentityModel.Tokens)
                //used to define rules for validating JWT tokens, such as checking the issuer, audience, lifetime,
                //and signature. It is crucial for security in APIs and web apps,
                //usually configured within JwtBearerOptions to set validation keys and expected values.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    //Key Configuration Properties
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]!))
                };

                //new JwtBearerEvents() is used in ASP.NET Core to provide a class instance with customizable callback methods
                //(events) that allow developers to hook into and control the JWT authentication process.
                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = authFailedContext =>
                    {
                        //OnAuthenticationFailed: Executes upon validation failure, enabling custom error logging
                        //or response header manipulation.
                        authFailedContext.NoResult();
                        authFailedContext.Response.StatusCode = 401;
                        authFailedContext.Response.ContentType = "text/plain";

                        return authFailedContext.Response.WriteAsync(authFailedContext.Exception.ToString());
                    },
                    OnChallenge = challengeContext =>
                    {
                        //OnChallenge: Invoked when a 401 Unauthorized response is triggered,
                        //allowing for customization of the response body or headers.
                        challengeContext.HandleResponse();
                        challengeContext.Response.ContentType = "application/json";
                        challengeContext.Response.StatusCode = 401;

                        return challengeContext.Response.WriteAsync("User is Unauthorized.");
                    },
                    OnForbidden = forbidContext =>
                    {
                        forbidContext.Response.StatusCode = 403;
                        forbidContext.Response.ContentType = "Application/json";

                        return forbidContext.Response.WriteAsync("Forbidden, Access is denied to to insufficient permissions.");
                    }
                };
            });

        }
    }
}
