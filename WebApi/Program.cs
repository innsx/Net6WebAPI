using Application;
using Application.Interfaces;
using Infastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Serilog;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer;
using System.Text;
using WebApi.ExceptionHandlers;
using WebApi.Services;
using WebApi.SharedServices;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;

var builder = WebApplication.CreateBuilder(args);

//declaring a logger configurations
var logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(builder.Configuration)  //this line tell .NET to READ logger configuratons setup in aspsettings.json 
    .CreateLogger();

////.Destructure. Serilog destructuring is the process of converting a complex .NET object into a structured format (like JSON)
//rather than simply calling its ToString() method.
//This allows you to log the object's internal properties,
//which is crucial for structured logging and analysis with tools like

try
{
    // Add services to the container.

    //OPTION1: these lines connects Serilog to .NET Built-in logging system (ILogger)
    //in ASP.NET Core Application
    Log.Logger = logger;
    builder.Host.UseSerilog(logger);

    //OPTION2: connects Serilog to .NET Built-in logging system
    //builder.Host.UseSerilog((context, loggerConfig) =>
    //loggerConfig.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    //builder.Services.AddSwaggerGen();  //Registering AddSwaggerGen method without adding additional options
    builder.Services.AddSwaggerExtension(); //Registering extension AddSwaggerExtension method

    //Registering Service Extension classes
    builder.Services.AddApplication();
    builder.Services.AppInfrastructure();
    builder.Services.AddPersistence(builder.Configuration);

    // WHEN NOT CREATING A SERVICEEXTENSION CLASS Register MediatR and scan for handlers in the current assembly 
    // USE THIS CONFIGURATION SETUP IN PROGRAM
    //builder.Services.AddMediatR(config =>
    //{
    //    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    //});

    //Scoped Lifetime: The instance lives for the duration of a single client request and is disposed of afterward.
    //Purpose: Typically used in Blazor or APIs to maintain state about the currently signed-in user,
    //accessed via constructor injection.
    builder.Services.AddScoped<IAuthenticatedUser, AuthenticatedUser>();

    //The code builder.Services.AddHttpContextAccessor();
    //is an ASP.NET Core method used to register the default implementation of
    //the IHttpContextAccessor interface with the dependency injection (DI) container.
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication(options =>
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
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        //DefaultChallengeScheme: The scheme used when an unauthenticated user tries to access
        //a protected resource (e.g., returning a 401 Unauthorized response).
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
            ValidIssuer = builder.Configuration["JWTSettings:Issuer"],
            ValidAudience = builder.Configuration["JWTSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:Key"]!))
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
                authFailedContext.Response.StatusCode = 500;
                authFailedContext.Response.ContentType = "text/plain";

                return authFailedContext.Response.WriteAsync(authFailedContext.Exception.ToString());
            },
            OnChallenge = challengeContext =>
            {
                //OnChallenge: Invoked when a 401 Unauthorized response is triggered,
                //allowing for customization of the response body or headers.
                challengeContext.HandleResponse();
                challengeContext.Response.ContentType = "text/plain";
                challengeContext.Response.StatusCode = 401;

                return challengeContext.Response.WriteAsync("User is Unauthorized.");
            },
            OnForbidden = forbidContext =>
            {
                forbidContext.Response.StatusCode = 403;
                forbidContext.Response.ContentType = "text/plain";

                return forbidContext.Response.WriteAsync("Forbidden, Access is denied to to insufficient permissions.");
            }
        };
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //USING AUTHENTICATION
    app.UseAuthentication();

    app.UseAuthorization();

    //Registering ErrorHandlerInMiddleware as a Service & Injecting an instance when requested
    app.UseMiddleware<ErrorHandlerInMiddleware>();

    //Use Serilog
    app.UseSerilogRequestLogging();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Something went wrong");
}
finally
{
    await Log.CloseAndFlushAsync();
}





//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(o =>
//{
//    o.RequireHttpsMetadata = false;
//    o.SaveToken = false;
//    o.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ClockSkew = TimeSpan.Zero,
//        ValidIssuer = builder.Configuration["JWTSettings:Issuer"],
//        ValidAudience = builder.Configuration["JWTSettings:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:Key"]))
//    };
//    o.Events = new JwtBearerEvents()
//    {
//        OnAuthenticationFailed = c =>
//        {
//            c.NoResult();
//            c.Response.StatusCode = 500;
//            c.Response.ContentType = "text/plain";
//            return c.Response.WriteAsync(c.Exception.ToString());
//        },
//        OnChallenge = context =>
//        {
//            context.HandleResponse();
//            context.Response.StatusCode = 401;
//            context.Response.ContentType = "text/plain";
//            return context.Response.WriteAsync("User unauthorized");
//        },
//        OnForbidden = context =>
//        {
//            context.Response.StatusCode = 403;
//            context.Response.ContentType = "text/plain";
//            return context.Response.WriteAsync("Access is denied due to insufficient permissions. ");
//        },
//    };
//});





//configure Serilog
//var columnOpts = new ColumnOptions();
//columnOpts.Store.Remove(StandardColumn.Properties);
//columnOpts.Store.Add(StandardColumn.LogEvent);
//columnOpts.LogEvent.DataLength = 2048;
//columnOpts.TimeStamp.NonClusteredIndex = true;

//Log.Logger = new LoggerConfiguration()
//        .MinimumLevel.Information()
//        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
//        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error)
//        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Error)
//        .WriteTo.Console()
//        .WriteTo.Debug()
//        .WriteTo.MSSqlServer(
//            connectionString: builder.Configuration.GetConnectionString("DbConnStrg"),
//            sinkOptions: new MSSqlServerSinkOptions { TableName = "SerilogEvents", AutoCreateSqlTable = true },
//            columnOptions: columnOpts
//            )        
//        .WriteTo.File("Logs/MyAppLogs.txt", rollingInterval: RollingInterval.Day)
//        .WriteTo.DatadogLogs(builder.Configuration["DataDogSettings:ApiKey"], 
//                            builder.Configuration["DataDogSettings:Source"],
//                            builder.Configuration["DataDogSettings:Service"], Environment.MachineName,
//            new[] { builder.Configuration["DataDogSettings:Environment"], 
//                    builder.Configuration["DataDogSettings:Application"], 
//                    builder.Configuration["DataDogSettings:LocationCode"] })

//        .CreateLogger();

//Log.Information("Logging with Serilog!");


//builder.Host.UseSerilog();
