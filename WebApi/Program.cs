using Application;
using Application.Interfaces;
using Infastructure;
using Persistence;
using Serilog;
using WebApi.ExceptionHandlers;
using WebApi.Services;
using WebApi.SharedServices;

var builder = WebApplication.CreateBuilder(args);

//declaring a logger configurations
var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom
    //.Destructure
    .Configuration(builder.Configuration)  //this line tell .NET to READ logger configuratons setup in aspsettings.json 
    .CreateLogger();

    //.Destructure -- Serilog destructuring is the process of converting a complex .NET object
    //into a structured format (like JSON)
    //rather than simply calling its ToString() method.
    //This allows you to log the object's internal properties,
    //which is crucial for structured logging and analysis with tools like

try
{
    // ************ Add services to the container. ****************

    //OPTION1: these lines connects Serilog to .NET Built-in logging system (ILogger) in ASP.NET Core Application
    //Log.Logger = loggerConfiguration; is the standard way in Serilog to assign a configured Logger instance to the global static Log.Logger property.
    //It defines the logging pipeline, including minimum levels and sinks (destinations),
    //ensuring that global Log.Information() calls are directed to the configured sinks
    Log.Logger = loggerConfiguration;

    // this is used to configure Serilog as the sole logging provider in a .NET application's generic host,
    // replacing any default logging providers
    builder.Host.UseSerilog(loggerConfiguration);

    //OPTION2: This approach lets you dynamically change the logging setup without redeploying the application.
    //builder.Host.UseSerilog((context, loggerConfig) =>
    //loggerConfig.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        //this tells the NET Framework to DIABLED camelCase in JSON output
        // and perserve property names as DEFINED in C# classes
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

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

    builder.Services.AddAuthenticationServiceExt(builder.Configuration);
    // ************ Add services to the container ENDS. ****************


    // ************ this is the Middleware Pipeline phase. **************
    //You use the app object to define how the application responds to HTTP requests
    //using methods like app.UseStaticFiles(), app.UseRouting(), or app.MapGet()... etc.
    //Once this builder.Build() method is called, the DI container configurations is built. 
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




//************ Recommended Middlewares Execution Order ******************************************
//app.UseExceptionHandler();                   // 1. Catch all unhandled exceptions
//app.UseHttpsRedirection();                   // 2. Redirect HTTP → HTTPS
//app.UseRouting();                            // 3. Match routes
//app.UseCors();                               // 4. CORS headersapp.UseAuthentication();                     
//app.UseAuthentication();                     // 5. Establish identity
//app.UseAuthorization();                      // 6. Check permissions
//app.UseOutputCache();                        // 7. Serve cached responses
//app.UseRateLimiter();                        // 8. Enforce rate limits
//app.UseResponseCompression();                // 9. Compress responses
//app.UseMiddleware<RequestLoggingMiddleware>();// 10. Custom middleware                                                  
//app.UseSerilogRequestLogging();              //Use Serilog
//app.MapControllers();                        // 11. Execute endpoints


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
