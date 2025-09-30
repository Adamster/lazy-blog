using Lazy.Application.Behaviors;
using Lazy.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using FluentValidation;
using Lazy.App.OptionsSetup;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Domain.Entities;
using Lazy.Infrastructure.Authorization;
using Lazy.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using AssemblyReference = Lazy.Infrastructure.AssemblyReference;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Lazy.Application.Abstractions.Email;
using Lazy.Blog.Api.ErrorHandler;
using Lazy.Blog.Api.Extensions;
using Lazy.Blog.Api.OptionsSetup;
using Lazy.Domain.Entities.Identity;
using Lazy.Infrastructure.Services.Impl;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

string lazyCorsPolicyName = "lazy-blog";
var today = DateTime.Today;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File($"Logs\\{today.Year}\\{today.Month}\\{today.Day}\\Logs.log")
    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(),  TelemetryConverter.Events)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Host.UseSerilog();
    
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor();
    
    builder
        .Services
        .Scan(
            selector => selector
                .FromAssemblies(
                    AssemblyReference.Assembly,
                    Lazy.Persistence.AssemblyReference.Assembly)
                .AddClasses(false)
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .WithScopedLifetime());

    builder.Services.AddMediatR(configuration =>
    {
        configuration.RegisterServicesFromAssembly(Lazy.Application.AssemblyReference.Assembly);
    });

    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
    //TODO: add after adding few commands with Domain events
    //builder.Services.Decorate(typeof(INotificationHandler<>), typeof())

    builder.Services.AddValidatorsFromAssembly(
        Lazy.Application.AssemblyReference.Assembly,
        includeInternalTypes: true);

    string connectionString = builder.Configuration.GetConnectionString("Database")!;

    builder.Services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
    builder.Services.AddSingleton<IEmailService, SendGridEmailSender>();

    builder.Services.AddProblemDetails();
    
    builder.Services.AddDbContext<LazyBlogDbContext>(
        (sp, optionsBuilder) =>
        {
            optionsBuilder.UseSqlServer(connectionString);
        });

    builder.Services.AddDefaultIdentity<User>()
        .AddRoles<Role>()
        .AddEntityFrameworkStores<LazyBlogDbContext>();

    builder
        .Services
        .AddControllers()
        .AddApplicationPart(Lazy.Presentation.AssemblyReference.Assembly);

    OpenApi.AddOpenApi(builder.Services);

    builder.Services.AddCors(o => 
        o.AddPolicy(lazyCorsPolicyName, policyBuilder =>
    {
        policyBuilder.WithOrigins("https://notlazy.org")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    }));


    builder.Services.ConfigureApplicationCookie(o =>
    {
        o.Cookie.Name = "nl.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.Domain = ".notlazy.org";
        o.SlidingExpiration = true;
    });

    builder.Services
        .AddAuthentication(o =>
        {
            o.DefaultScheme = IdentityConstants.ApplicationScheme;
            o.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        })
       
        
        .AddJwtBearer()
        .AddExternalAuthentication(builder.Configuration);



    builder.Services.ConfigureOptions<JwtOptionsSetup>();
    builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
    builder.Services.ConfigureOptions<AzureBlobStorageOptionsSetup>();
    builder.Services.ConfigureOptions<SendGridOptionsSetup>();

    var app = builder.Build();

    CreateDbIfNotExists(app);
    
   
    
    app.UseHttpsRedirection();

    app.UseCors(lazyCorsPolicyName);

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapOpenApi();

    app.MapControllers();

    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.DeepSpace));

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    
    
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


//TODO: Remove this in future
static void CreateDbIfNotExists(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LazyBlogDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}